using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// Base for anything a unit can hold, carry, craft or place.
    /// Noise is N0 in the noise model (source strength before door and corridor attenuation).
    /// Heat is the signature seen by heat vision (0–10).
    /// </summary>
    public abstract class ItemDefinition : DefinitionBase
    {
        [Header("Item")]
        [SerializeField] internal Rarity rarity = Rarity.Common;
        [SerializeField] internal Mark mark = Mark.MkI;

        [Tooltip("Noise N0 emitted per use (hit, shot, throw, tool tick).")]
        [Min(0f)] [SerializeField] internal float noiseOnUse;

        [Tooltip("Heat signature while in use, 0–10.")]
        [Range(0f, 10f)] [SerializeField] internal float heatOnUse;

        [SerializeField] internal LightEmission lightOnUse = LightEmission.None;

        public abstract ItemCategory Category { get; }
        public Rarity Rarity => rarity;
        public Mark Mark => mark;
        public float NoiseOnUse => noiseOnUse;
        public float HeatOnUse => heatOnUse;
        public LightEmission LightOnUse => lightOnUse;

        protected sealed override void ValidateDefinition(ValidationReport report)
        {
            report.NonNegative(this, nameof(noiseOnUse), noiseOnUse);
            report.InRange(this, nameof(heatOnUse), heatOnUse, 0f, 10f);
            ValidateItem(report);
        }

        protected abstract void ValidateItem(ValidationReport report);
    }
}
