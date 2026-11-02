using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>One change to a unit stat, from an augment, a specialization or an effect.</summary>
    [Serializable]
    public struct StatModifier
    {
        public UnitStat stat;
        public ModifierOperation operation;
        public float amount;

        public StatModifier(UnitStat stat, ModifierOperation operation, float amount)
        {
            this.stat = stat;
            this.operation = operation;
            this.amount = amount;
        }

        public static StatModifier Add(UnitStat stat, float amount) => new StatModifier(stat, ModifierOperation.Add, amount);

        public static StatModifier Percent(UnitStat stat, float fraction) => new StatModifier(stat, ModifierOperation.AddPercent, fraction);

        public override string ToString() => operation == ModifierOperation.Add
            ? $"{stat} {amount:+0.##;-0.##}"
            : $"{stat} {amount:+0%;-0%}";
    }

    /// <summary>Applies modifier sets. Pure and allocation-free; safe on any thread.</summary>
    public static class StatMath
    {
        /// <summary>
        /// Applies every modifier for <paramref name="stat"/>: all flat additions first, then the summed
        /// percentage (so two +15% augments give +30%, not +32.25%).
        /// </summary>
        public static float Apply(UnitStat stat, float baseValue, IReadOnlyList<StatModifier> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return baseValue;
            }

            var flat = 0f;
            var percent = 0f;
            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier.stat != stat)
                {
                    continue;
                }

                if (modifier.operation == ModifierOperation.Add)
                {
                    flat += modifier.amount;
                }
                else
                {
                    percent += modifier.amount;
                }
            }

            return (baseValue + flat) * (1f + percent);
        }

        /// <summary>Sum of flat additions for <paramref name="stat"/> (for stats with no base, like crit chance).</summary>
        public static float SumFlat(UnitStat stat, IReadOnlyList<StatModifier> modifiers) => Apply(stat, 0f, modifiers);
    }
}
