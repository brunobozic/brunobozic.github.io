using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A valuable resource hauled out of a hulk (grav-lattice alloy). Value can step up for
    /// later units, which drives the "greed vs the timer" choice.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Resource", fileName = "itm_resource_")]
    public sealed class ResourceDefinition : ItemDefinition
    {
        [Header("Resource")]
        [Min(1)] [SerializeField] internal int slotsPerUnit = 1;
        [Min(0)] [SerializeField] internal int scripValue = 400;

        [Tooltip("Units from this count onward (1-based) are worth the premium value. 0 = no premium.")]
        [Min(0)] [SerializeField] internal int premiumFromUnit;
        [Min(0)] [SerializeField] internal int premiumScripValue;

        public override ItemCategory Category => ItemCategory.Resource;
        public int SlotsPerUnit => slotsPerUnit;
        public int ScripValue => scripValue;
        public int PremiumFromUnit => premiumFromUnit;
        public int PremiumScripValue => premiumScripValue;

        /// <summary>Total Scrip for <paramref name="units"/> units, applying the premium tier.</summary>
        public int TotalScripValue(int units)
        {
            if (units <= 0)
            {
                return 0;
            }

            if (premiumFromUnit <= 0 || units < premiumFromUnit)
            {
                return checked(units * scripValue);
            }

            var standardUnits = premiumFromUnit - 1;
            var premiumUnits = units - standardUnits;
            return checked(standardUnits * scripValue + premiumUnits * premiumScripValue);
        }

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(slotsPerUnit), slotsPerUnit);
            if (premiumFromUnit > 0 && premiumScripValue < scripValue)
            {
                report.Warning(this, nameof(premiumScripValue), "is lower than the standard value.");
            }
        }
    }
}
