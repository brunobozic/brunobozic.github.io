using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// Mission cargo (fuel cylinders, cryo fuel cells). Condition drops over time in hot rooms.
    /// <see cref="conditionLossPerSecondByHeat"/> is indexed by Heat intensity 0–3.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Cargo", fileName = "itm_cargo_")]
    public sealed class CargoDefinition : ItemDefinition
    {
        public const int HeatIntensityLevels = 4;

        [Header("Cargo")]
        [Min(1)] [SerializeField] internal int slots = 1;
        [Min(1f)] [SerializeField] internal float maxCondition = 100f;

        [Tooltip("Condition lost per second at Heat intensity 0, 1, 2 and 3.")]
        [SerializeField] internal float[] conditionLossPerSecondByHeat = new float[HeatIntensityLevels];

        [SerializeField] internal bool explodesWhenDestroyed;
        [Min(0f)] [SerializeField] internal float explosionDamage;
        [Min(0f)] [SerializeField] internal float explosionRadius;

        public override ItemCategory Category => ItemCategory.Cargo;
        public int Slots => slots;
        public float MaxCondition => maxCondition;
        public bool ExplodesWhenDestroyed => explodesWhenDestroyed;
        public float ExplosionDamage => explosionDamage;
        public float ExplosionRadius => explosionRadius;

        /// <summary>Condition lost per second at a Heat intensity (clamped to 0–3).</summary>
        public float ConditionLossPerSecond(int heatIntensity)
        {
            if (conditionLossPerSecondByHeat == null || conditionLossPerSecondByHeat.Length == 0)
            {
                return 0f;
            }

            var index = Mathf.Clamp(heatIntensity, 0, conditionLossPerSecondByHeat.Length - 1);
            return conditionLossPerSecondByHeat[index];
        }

        /// <summary>Seconds a full-condition item survives at a Heat intensity; infinity if it doesn't degrade.</summary>
        public float SecondsToRupture(int heatIntensity)
        {
            var loss = ConditionLossPerSecond(heatIntensity);
            return loss > 0f ? maxCondition / loss : float.PositiveInfinity;
        }

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(slots), slots);
            report.Positive(this, nameof(maxCondition), maxCondition);

            if (conditionLossPerSecondByHeat == null || conditionLossPerSecondByHeat.Length != HeatIntensityLevels)
            {
                report.Error(this, nameof(conditionLossPerSecondByHeat), $"must have exactly {HeatIntensityLevels} entries (Heat 0–3).");
            }
            else
            {
                for (var i = 0; i < conditionLossPerSecondByHeat.Length; i++)
                {
                    report.NonNegative(this, $"{nameof(conditionLossPerSecondByHeat)}[{i}]", conditionLossPerSecondByHeat[i]);
                    if (i > 0 && conditionLossPerSecondByHeat[i] < conditionLossPerSecondByHeat[i - 1])
                    {
                        report.Warning(this, nameof(conditionLossPerSecondByHeat), $"Heat {i} damages less than Heat {i - 1}.");
                    }
                }
            }

            if (explodesWhenDestroyed)
            {
                report.Positive(this, nameof(explosionDamage), explosionDamage);
                report.Positive(this, nameof(explosionRadius), explosionRadius);
            }
        }
    }
}
