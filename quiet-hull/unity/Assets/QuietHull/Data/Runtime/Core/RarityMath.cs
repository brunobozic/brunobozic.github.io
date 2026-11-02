using System;

namespace QuietHull.Data
{
    /// <summary>
    /// The rarity math from the design doc (Content lists ▸ Rarity math). Benefits scale by m,
    /// handicaps and Threat by h. Pure functions over constant tables; thread-safe.
    /// </summary>
    public static class RarityMath
    {
        private static readonly float[] BenefitFactors = { 1f, 1.5f, 2f, 2.5f, 3f };
        private static readonly float[] HandicapFactors = { 1f, 1.25f, 1.5f, 1.75f, 2f };
        private static readonly float[] BrutalityRatesPercent = { 4f, 6f, 8f, 10f, 12f };

        /// <summary>Benefit multiplier m.</summary>
        public static float M(Rarity rarity) => BenefitFactors[Index(rarity)];

        /// <summary>Handicap and Threat multiplier h.</summary>
        public static float H(Rarity rarity) => HandicapFactors[Index(rarity)];

        public static float Factor(OmenSide side, Rarity rarity) => side == OmenSide.Benefit ? M(rarity) : H(rarity);

        /// <summary>Highest card level (Level I–V); duplicates fill the bar, the Cartomancer levels it.</summary>
        public const int MaxCardLevel = 5;

        /// <summary>Each card level adds this to m. Level V (m + 0.4) stays below the next rarity's Level I (m + 0.5).</summary>
        public const float LevelBenefitStep = 0.1f;

        /// <summary>Each card level adds this to h. Level V (h + 0.2) stays below the next rarity's Level I (h + 0.25).</summary>
        public const float LevelHandicapStep = 0.05f;

        /// <summary>Benefit multiplier at a card level: m + 0.1 × (level − 1).</summary>
        public static float M(Rarity rarity, int level) => M(rarity) + LevelBenefitStep * (ClampLevel(level) - 1);

        /// <summary>Handicap multiplier at a card level: h + 0.05 × (level − 1).</summary>
        public static float H(Rarity rarity, int level) => H(rarity) + LevelHandicapStep * (ClampLevel(level) - 1);

        public static float Factor(OmenSide side, Rarity rarity, int level) => side == OmenSide.Benefit ? M(rarity, level) : H(rarity, level);

        /// <summary>Card Threat at a level: base × h(level), rounded half away from zero.</summary>
        public static int ScaleThreat(int baseThreat, Rarity rarity, int level) =>
            (int)Math.Round(baseThreat * H(rarity, level), MidpointRounding.AwayFromZero);

        private static int ClampLevel(int level) => level < 1 ? 1 : level > MaxCardLevel ? MaxCardLevel : level;

        /// <summary>Card Threat: base × h, rounded half away from zero (−4.5 becomes −5).</summary>
        public static int ScaleThreat(int baseThreat, Rarity rarity) =>
            (int)Math.Round(baseThreat * H(rarity), MidpointRounding.AwayFromZero);

        /// <summary>Brutality payout per point of the card's Threat, in percent (4 / 6 / 8 / 10 / 12).</summary>
        public static float BrutalityRatePercent(Rarity rarity) => BrutalityRatesPercent[Index(rarity)];

        /// <summary>
        /// Scales a Common value by a factor using one of the doc's rules. Chance and Reduction take
        /// and return fractions (0.4 = 40%); Linear takes percent or plain numbers.
        /// </summary>
        public static float Scale(float commonValue, ScalingRule rule, float factor)
        {
            switch (rule)
            {
                case ScalingRule.None:
                    return commonValue;
                case ScalingRule.Linear:
                case ScalingRule.Count:
                    return commonValue * factor;
                case ScalingRule.Structural:
                    return (float)Math.Floor(commonValue * factor + 1e-4f);
                case ScalingRule.Multiplier:
                    return 1f + (commonValue - 1f) * factor;
                case ScalingRule.Chance:
                case ScalingRule.Reduction:
                    return 1f - (float)Math.Pow(1f - Clamp01(commonValue), factor);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rule), rule, null);
            }
        }

        /// <summary>Rounds a displayed percentage the way the doc tables do (half away from zero).</summary>
        public static int RoundPercent(float percent) => (int)Math.Round(percent, MidpointRounding.AwayFromZero);

        private static int Index(Rarity rarity)
        {
            var index = (int)rarity;
            if (index < 0 || index >= BenefitFactors.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null);
            }

            return index;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
