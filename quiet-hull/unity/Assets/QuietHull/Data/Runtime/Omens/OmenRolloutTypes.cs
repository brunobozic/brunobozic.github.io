using System;

namespace QuietHull.Data
{
    /// <summary>One card's arrival in the campaign: which mission, how it is found, and when it becomes playable.</summary>
    [Serializable]
    public struct OmenRolloutEntry
    {
        public int mission;
        public OmenCardDefinition card;
        public OmenSource source;
        [UnityEngine.Tooltip("First mission the card can be slotted in; at least the mission it arrives in.")]
        public int playableFromMission;
        [UnityEngine.Tooltip("Only on these story paths (any of these campaign flags, such as ledger_sold); empty = every path.")]
        public string[] requiresAnyFlag;

        public OmenRolloutEntry(int mission, OmenCardDefinition card, OmenSource source, int playableFromMission = 0, params string[] requiresAnyFlag)
        {
            this.mission = mission;
            this.card = card;
            this.source = source;
            this.playableFromMission = playableFromMission > 0 ? playableFromMission : mission;
            this.requiresAnyFlag = requiresAnyFlag ?? Array.Empty<string>();
        }

        public bool IsPathBound => requiresAnyFlag != null && requiresAnyFlag.Length > 0;

        public bool IsOnPath(Func<string, bool> hasFlag)
        {
            if (!IsPathBound)
            {
                return true;
            }

            if (hasFlag == null)
            {
                return false;
            }

            for (var i = 0; i < requiresAnyFlag.Length; i++)
            {
                if (hasFlag(requiresAnyFlag[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>What the Omen system offers during one mission: slots, drops, rarity ceilings, combining.</summary>
    [Serializable]
    public struct MissionOmenSchedule
    {
        public int mission;
        public string workingTitle;
        [UnityEngine.Tooltip("Omen slots while playing this mission.")]
        public int slots;
        [UnityEngine.Tooltip("Omen slots once the mission is complete (the next mission starts with these).")]
        public int slotsAfter;
        [UnityEngine.Tooltip("Requisition Omen drops per run (0 = none).")]
        public int dropsMin;
        public int dropsMax;
        [UnityEngine.Tooltip("Highest rarity a random drop can roll this mission.")]
        public Rarity dropCeiling;
        [UnityEngine.Tooltip("A card of this rarity is guaranteed once (the Sephora's Rare), if set.")]
        public bool guaranteesRarity;
        public Rarity guaranteedRarity;
        [UnityEngine.Tooltip("The Cartomancer levels cards (I–V) and ascends Level V cards.")]
        public bool cartomancerOpen;
        [UnityEngine.Tooltip("Highest rarity a card can ascend to.")]
        public Rarity ascendCeiling;
        [UnityEngine.Tooltip("Catalysts work on owned gear at the fabricator bench (Strip, Temper, Seal drop).")]
        public bool benchOpen;
        [UnityEngine.Tooltip("Catalysts mutate Omen cards: Reroll, Deepen, Refine.")]
        public bool mutationsOpen;
        [UnityEngine.Tooltip("Hive-tainted corrupts cards and infuses gear with amalgam.")]
        public bool corruptionOpen;
        [UnityEngine.Tooltip("The mission has a set-piece whose script locks the Major slot.")]
        public bool hasSetPieceLockingMajor;
        [UnityEngine.Tooltip("Commander countermeasure cards in hand (Vent, Seal Door, ...).")]
        public int countermeasureHand;

        public bool HasDrops => dropsMax > 0;
    }
}
