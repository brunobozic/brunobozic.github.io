using System;

namespace QuietHull.Data
{
    /// <summary>
    /// Amalgam infusion: Hive-tainted on an item already owned, soaked in alien blood. Irreversible, four
    /// outcomes at 25% each: Nothing, Overgrowth (+1 Mark, up to one past the normal maximum), Xenomorphic (an alien
    /// implicit) and Rot (an affix turns into a drawback). Rarity shrinks the risk: from Rare, Rot spares
    /// sockets; a Legendary picks the better of two rolls. Pure; thread-safe.
    /// </summary>
    public static class InfusionMath
    {
        /// <summary>Collectors at Patusan pay this much more for an Infused item.</summary>
        public const float InfusedSaleMultiplier = 1.2f;

        /// <summary>The first mission infusion is possible (after the Brood Mother; the amalgam is hers).</summary>
        public const int OpensAtMission = 9;

        /// <summary>The outcome a roll in [0, 1) lands on: four equal quarters.</summary>
        public static InfusionOutcome OutcomeOf(float roll)
        {
            roll = roll < 0f ? 0f : roll >= 1f ? 0.999999f : roll;
            return (InfusionOutcome)(int)(roll * 4f);
        }

        /// <summary>
        /// Infuses an item at <paramref name="mark"/> whose line normally stops at <paramref name="normalMaximum"/>.
        /// A Legendary Catalyst rolls twice (<paramref name="secondRoll"/>) and the player keeps the better result.
        /// </summary>
        public static InfusionResult Infuse(Rarity catalystRarity, Mark mark, Mark normalMaximum, float roll, float secondRoll = 0f)
        {
            var outcome = OutcomeOf(roll);
            if (catalystRarity >= Rarity.Legendary)
            {
                var other = OutcomeOf(secondRoll);
                if (Preference(other) > Preference(outcome))
                {
                    outcome = other;
                }
            }

            Mark? newMark = null;
            if (outcome == InfusionOutcome.Overgrowth)
            {
                // +1 Mark, allowed to reach one past the line's normal maximum (an Mk VIII augment becomes Mk IX).
                var next = Math.Min((int)mark + 1, (int)normalMaximum + 1);
                newMark = (Mark)Math.Min(next, (int)Mark.MkXII);
            }

            return new InfusionResult(outcome, newMark, catalystRarity >= Rarity.Rare);
        }

        /// <summary>How a player ranks outcomes when choosing (Legendary): growth first, Rot last.</summary>
        private static int Preference(InfusionOutcome outcome)
        {
            switch (outcome)
            {
                case InfusionOutcome.Overgrowth: return 3;
                case InfusionOutcome.Xenomorphic: return 2;
                case InfusionOutcome.Nothing: return 1;
                default: return 0;
            }
        }
    }
}
