using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A Syndicate-style body augment for a synthetic. One per slot; a higher mark replaces a lower one.
    /// Augments are items, so they can be bought, carried and crafted from blueprints.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Augment", fileName = "aug_")]
    public sealed class AugmentDefinition : ItemDefinition
    {
        [Header("Augment")]
        [SerializeField] internal AugmentSlot slot;
        [SerializeField] internal AugmentKind kind = AugmentKind.Cyber;
        [Tooltip("Clinic price; 0 = not sold, crafted only.")]
        [Min(0)] [SerializeField] internal int priceScrip = 450;
        [SerializeField] internal StatModifier[] modifiers = Array.Empty<StatModifier>();

        public override ItemCategory Category => ItemCategory.Augment;
        public AugmentSlot Slot => slot;
        public AugmentKind Kind => kind;
        public int PriceScrip => priceScrip;
        public bool SoldAtClinic => priceScrip > 0;
        public IReadOnlyList<StatModifier> Modifiers => modifiers ?? Array.Empty<StatModifier>();

        /// <summary>True when this augment would replace <paramref name="installed"/> as an upgrade.</summary>
        public bool Upgrades(AugmentDefinition installed) => installed == null || (installed.slot == slot && Mark > installed.Mark);

        protected override void ValidateItem(ValidationReport report)
        {
            report.NotEmpty(this, nameof(modifiers), modifiers);
            var seen = new HashSet<UnitStat>();
            foreach (var modifier in Modifiers)
            {
                if (!seen.Add(modifier.stat))
                {
                    report.Warning(this, nameof(modifiers), $"{modifier.stat} is modified twice; merge the entries.");
                }

                if (modifier.amount == 0f)
                {
                    report.Warning(this, nameof(modifiers), $"{modifier.stat} has a zero amount.");
                }
            }
        }
    }
}
