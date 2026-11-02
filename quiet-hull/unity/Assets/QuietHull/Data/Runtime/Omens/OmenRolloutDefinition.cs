using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// The tarot rollout: which Omen cards arrive in which mission, how many slots are open, when random
    /// drops start and how rare they can be, and when the Cartomancer's table opens. Requisition drops
    /// only ever come from cards the player has already met, so every new card arrives as an event.
    /// Read-only at runtime; every query allocates its own result, so it is safe from any thread.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Omens/Omen Rollout", fileName = "rollout_")]
    public sealed class OmenRolloutDefinition : DefinitionBase
    {
        public const int MaxSlots = 4;

        [SerializeField] internal MissionOmenSchedule[] schedule = Array.Empty<MissionOmenSchedule>();
        [SerializeField] internal OmenRolloutEntry[] entries = Array.Empty<OmenRolloutEntry>();

        [Header("Slots")]
        [Tooltip("Commander rank that opens the third slot (replaces 'the first Omens at Rank 7').")]
        [Min(1)] [SerializeField] internal int thirdSlotRank = 7;
        [Tooltip("The 4th slot comes from the Reliquarian skill node, not from the schedule.")]
        [SerializeField] internal string fourthSlotNode = "Reliquarian";

        [Header("Mercy")]
        [Tooltip("Offered free after this many failed runs in a row; never takes a slot.")]
        [Min(1)] [SerializeField] internal int mercyAfterFailures = 2;
        [SerializeField] internal OmenCardDefinition mercyCard;
        [Min(1)] [SerializeField] internal int mercyFromMission = 2;

        public IReadOnlyList<MissionOmenSchedule> Schedule => schedule ?? Array.Empty<MissionOmenSchedule>();
        public IReadOnlyList<OmenRolloutEntry> Entries => entries ?? Array.Empty<OmenRolloutEntry>();
        public int ThirdSlotRank => thirdSlotRank;
        public string FourthSlotNode => fourthSlotNode;
        public int MercyAfterFailures => mercyAfterFailures;
        public OmenCardDefinition MercyCard => mercyCard;
        public int MercyFromMission => mercyFromMission;
        public int LastMission => Schedule.Count == 0 ? 0 : Schedule[Schedule.Count - 1].mission;

        public bool TryGetSchedule(int mission, out MissionOmenSchedule result)
        {
            var list = Schedule;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].mission == mission)
                {
                    result = list[i];
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>Cards that arrive in exactly this mission on the player's path.</summary>
        public List<OmenRolloutEntry> ArrivingIn(int mission, Func<string, bool> hasFlag = null) =>
            Collect(e => e.mission == mission && e.IsOnPath(hasFlag));

        /// <summary>Every card met by the end of <paramref name="mission"/> on the player's path (face-down ones too).</summary>
        public List<OmenCardDefinition> MetBy(int mission, Func<string, bool> hasFlag = null) =>
            Cards(e => e.mission <= mission && e.IsOnPath(hasFlag));

        /// <summary>Cards that can be slotted during <paramref name="mission"/>.</summary>
        public List<OmenCardDefinition> PlayableIn(int mission, Func<string, bool> hasFlag = null) =>
            Cards(e => e.playableFromMission <= mission && e.mission <= mission && e.IsOnPath(hasFlag));

        /// <summary>
        /// The Requisition drop pool for a run of <paramref name="mission"/>: cards met before it starts and playable
        /// in it. Empty while drops are off. Rarity is rolled separately, capped at the schedule's ceiling.
        /// </summary>
        public List<OmenCardDefinition> DropPool(int mission, Func<string, bool> hasFlag = null)
        {
            if (!TryGetSchedule(mission, out var s) || !s.HasDrops)
            {
                return new List<OmenCardDefinition>(0);
            }

            return Cards(e => e.mission < mission && e.playableFromMission <= mission && e.IsOnPath(hasFlag));
        }

        /// <summary>Slots open during <paramref name="mission"/>, with the rank gate and the Reliquarian node applied.</summary>
        public int SlotsFor(int mission, int commanderRank, bool hasFourthSlotNode)
        {
            if (!TryGetSchedule(mission, out var s))
            {
                return 0;
            }

            var slots = s.slots;
            if (slots >= 3 && commanderRank < thirdSlotRank)
            {
                slots = 2;
            }

            if (hasFourthSlotNode && slots == 3)
            {
                slots = MaxSlots;
            }

            return slots;
        }

        public bool MercyAvailable(int mission, int failuresInARow) =>
            mercyCard != null && mission >= mercyFromMission && failuresInARow >= mercyAfterFailures;

        private List<OmenRolloutEntry> Collect(Predicate<OmenRolloutEntry> match)
        {
            var list = Entries;
            var result = new List<OmenRolloutEntry>(4);
            for (var i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                {
                    result.Add(list[i]);
                }
            }

            return result;
        }

        private List<OmenCardDefinition> Cards(Predicate<OmenRolloutEntry> match)
        {
            var list = Entries;
            var result = new List<OmenCardDefinition>(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].card != null && match(list[i]) && !result.Contains(list[i].card))
                {
                    result.Add(list[i].card);
                }
            }

            return result;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotEmpty(this, nameof(schedule), schedule);
            ValidateSchedule(report);
            ValidateEntries(report);
            report.NotNull(this, nameof(mercyCard), mercyCard);
            if (mercyCard != null && mercyCard.Threat > 0)
            {
                report.Error(this, nameof(mercyCard), "the Mercy card must make a run easier (Threat ≤ 0).");
            }
        }

        private void ValidateSchedule(ValidationReport report)
        {
            var list = Schedule;
            for (var i = 0; i < list.Count; i++)
            {
                var s = list[i];
                if (s.mission != i + 1)
                {
                    report.Error(this, nameof(schedule), $"row {i} is mission {s.mission}; missions must run 1, 2, 3 … without gaps.");
                }

                if (s.slots < 0 || s.slots > 3 || s.slotsAfter < s.slots || s.slotsAfter > 3)
                {
                    report.Error(this, nameof(schedule), $"mission {s.mission}: slots {s.slots} → {s.slotsAfter} must stay within 0..3 and never shrink (the 4th is the Reliquarian node).");
                }

                if (s.dropsMin < 0 || s.dropsMin > s.dropsMax)
                {
                    report.Error(this, nameof(schedule), $"mission {s.mission}: drops {s.dropsMin}–{s.dropsMax} are out of order.");
                }

                if (i == 0)
                {
                    continue;
                }

                var p = list[i - 1];
                if (s.slots < p.slotsAfter)
                {
                    report.Error(this, nameof(schedule), $"mission {s.mission} starts with fewer slots ({s.slots}) than mission {p.mission} ended with ({p.slotsAfter}).");
                }

                if (p.HasDrops && (!s.HasDrops || s.dropCeiling < p.dropCeiling || s.dropsMax < p.dropsMax))
                {
                    report.Error(this, nameof(schedule), $"mission {s.mission}: drops and their rarity ceiling never go back down.");
                }

                if (p.combiningOpen && (!s.combiningOpen || s.combineCeiling < p.combineCeiling))
                {
                    report.Error(this, nameof(schedule), $"mission {s.mission}: the Cartomancer's table never closes or lowers its ceiling.");
                }

                if (s.countermeasureHand < p.countermeasureHand)
                {
                    report.Error(this, nameof(schedule), $"mission {s.mission}: the countermeasure hand never shrinks.");
                }
            }
        }

        private void ValidateEntries(ValidationReport report)
        {
            var seen = new HashSet<OmenCardDefinition>();
            var last = LastMission;
            foreach (var e in Entries)
            {
                if (e.card == null)
                {
                    report.Error(this, nameof(entries), $"mission {e.mission}: an entry has no card.");
                    continue;
                }

                if (!seen.Add(e.card))
                {
                    report.Error(this, nameof(entries), $"{e.card.Id} arrives more than once.");
                }

                if (e.mission < 1 || e.mission > last)
                {
                    report.Error(this, nameof(entries), $"{e.card.Id}: mission {e.mission} is outside the schedule (1..{last}).");
                }

                if (e.playableFromMission < e.mission)
                {
                    report.Error(this, nameof(entries), $"{e.card.Id}: playable before it arrives.");
                }

                if (e.source == OmenSource.FaceDown && e.playableFromMission == e.mission)
                {
                    report.Error(this, nameof(entries), $"{e.card.Id}: a face-down card is read later, not played at once.");
                }

                if (TryGetSchedule(e.playableFromMission, out var s) && s.slots == 0)
                {
                    report.Error(this, nameof(entries), $"{e.card.Id}: playable in mission {e.playableFromMission}, which has no slots.");
                }
            }
        }
    }
}
