using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// The rules for a run's Omen loadout: one Major (none on a set-piece that locks it), Minors in the
    /// remaining slots, no two Minors of one suit, no card twice. And the stacking rule: within one family
    /// (or one suit), only the strongest card's flat bonus counts in full; every other one counts half.
    /// Pure and allocation-light; thread-safe.
    /// </summary>
    public static class OmenLoadoutRules
    {
        /// <summary>What a second (third, ...) card of the same family adds of its value.</summary>
        public const float SameFamilyShare = 0.5f;

        /// <summary>Problems with a loadout; empty when it is legal.</summary>
        public static List<string> Check(IReadOnlyList<OmenCardInstance> cards, int totalSlots, bool majorLocked)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            var problems = new List<string>(2);
            var majorSlots = totalSlots > 0 && !majorLocked ? 1 : 0;
            var minorSlots = Math.Max(0, totalSlots - 1);
            var majors = 0;
            var minors = 0;
            var suits = new HashSet<OmenSuit>();
            var definitions = new HashSet<OmenCardDefinition>();

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null)
                {
                    problems.Add($"slot {i} is empty.");
                    continue;
                }

                if (!definitions.Add(card.Definition))
                {
                    problems.Add($"{card.Definition.DisplayName} is slotted twice.");
                }

                if (card.Arcana == OmenArcana.Major)
                {
                    majors++;
                }
                else
                {
                    minors++;
                    if (!suits.Add(card.Suit))
                    {
                        problems.Add($"two Minor cards of {card.Suit}.");
                    }
                }
            }

            if (majors > majorSlots)
            {
                problems.Add(majorLocked ? "this set-piece locks the Major slot." : "only one Major card per run.");
            }

            if (minors > minorSlots)
            {
                problems.Add($"{minors} Minor cards, {minorSlots} Minor slots open.");
            }

            return problems;
        }

        /// <summary>
        /// The loadout's total for one flat effect (e.g. LootPercent), with the stacking rule: per family
        /// (Minors: per suit), the largest value counts in full and each other counts <see cref="SameFamilyShare"/>.
        /// </summary>
        public static float Stacked(IReadOnlyList<OmenCardInstance> cards, OmenEffectKind kind)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            var byGroup = new Dictionary<int, List<float>>();
            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null)
                {
                    continue;
                }

                var value = card.ValueOf(kind);
                if (value == 0f)
                {
                    continue;
                }

                var key = card.Arcana == OmenArcana.Major ? (int)card.Family : 100 + (int)card.Suit;
                if (!byGroup.TryGetValue(key, out var values))
                {
                    byGroup[key] = values = new List<float>(2);
                }

                values.Add(value);
            }

            var total = 0f;
            foreach (var values in byGroup.Values)
            {
                values.Sort((a, b) => Math.Abs(b).CompareTo(Math.Abs(a)));
                for (var i = 0; i < values.Count; i++)
                {
                    total += i == 0 ? values[i] : values[i] * SameFamilyShare;
                }
            }

            return total;
        }
    }
}
