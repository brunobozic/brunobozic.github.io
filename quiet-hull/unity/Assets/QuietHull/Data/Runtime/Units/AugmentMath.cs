using System;

namespace QuietHull.Data
{
    /// <summary>
    /// How augments scale with mark (Mk I–VIII for synth augments, Mk I–XII for worker modules).
    /// Pure functions over the design tables; thread-safe.
    /// </summary>
    public static class AugmentMath
    {
        public const int MaxSynthAugmentMark = 8;
        public const int MaxWorkerModuleMark = 12;
        public const int MaxShieldingRating = 3;

        /// <summary>Effect multiplier: +30% per mark (Mk VIII ×3.1).</summary>
        public static float EffectFactor(Mark mark) => 1f + 0.3f * (Level(mark) - 1);

        /// <summary>Price multiplier: ×1.7 per mark (Mk VIII ≈ ×41).</summary>
        public static float PriceFactor(Mark mark) => (float)Math.Pow(1.7, Level(mark) - 1);

        /// <summary>Weight grows for heavy parts and shrinks for infiltration parts as marks rise.</summary>
        public static float WeightFactor(AugmentLine line, Mark mark)
        {
            var steps = Level(mark) - 1;
            switch (line)
            {
                case AugmentLine.Assault: return 1f + 0.15f * steps;
                case AugmentLine.Standard: return 1f + 0.05f * steps;
                case AugmentLine.Fine: return 1f;
                case AugmentLine.Shadow: return Math.Max(0.5f, 1f - 0.05f * steps);
                default: throw new ArgumentOutOfRangeException(nameof(line), line, null);
            }
        }

        /// <summary>Power draw and output: +20% per mark (Shadow parts +10%).</summary>
        public static float PowerFactor(AugmentLine line, Mark mark) =>
            1f + (line == AugmentLine.Shadow ? 0.1f : 0.2f) * (Level(mark) - 1);

        /// <summary>
        /// Signature (noise, heat, vibration, aggro) multiplier. Assault parts get louder as they get
        /// stronger; Shadow parts suppress more; Standard and Fine stay flat.
        /// </summary>
        public static float SignatureFactor(AugmentLine line, Mark mark)
        {
            var steps = Level(mark) - 1;
            switch (line)
            {
                case AugmentLine.Assault: return 1f + 0.15f * steps;
                case AugmentLine.Shadow: return EffectFactor(mark);
                default: return 1f;
            }
        }

        /// <summary>Fabricator tier needed: Mk I–II T1, III–IV T2, V–VI T3, VII T4, VIII and above T5.</summary>
        public static int RequiredFabricatorTier(Mark mark)
        {
            var level = Level(mark);
            if (level <= 2) return 1;
            if (level <= 4) return 2;
            if (level <= 6) return 3;
            return level == 7 ? 4 : 5;
        }

        /// <summary>
        /// Condition shielding doesn't multiply: it steps up one rating every four marks
        /// (Mk I–IV +base, V–VIII +1, IX–XII +2), capped at 3.
        /// </summary>
        public static float ShieldingAt(float commonRating, Mark mark) =>
            Math.Min(MaxShieldingRating, commonRating + (Level(mark) - 1) / 4);

        public static bool IsShieldingStat(UnitStat stat) =>
            stat == UnitStat.HeatShielding || stat == UnitStat.ColdShielding || stat == UnitStat.GravityShielding ||
            stat == UnitStat.RadiationShielding || stat == UnitStat.AcidShielding || stat == UnitStat.IonShielding;

        /// <summary>Scales one Mk I modifier to a mark.</summary>
        public static StatModifier Scale(StatModifier common, Mark mark) =>
            IsShieldingStat(common.stat)
                ? new StatModifier(common.stat, common.operation, ShieldingAt(common.amount, mark))
                : new StatModifier(common.stat, common.operation, common.amount * EffectFactor(mark));

        private static int Level(Mark mark)
        {
            var level = (int)mark;
            if (level < 1 || level > MaxWorkerModuleMark)
            {
                throw new ArgumentOutOfRangeException(nameof(mark), mark, null);
            }

            return level;
        }
    }
}
