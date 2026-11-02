using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A Syndicate-style body augment for a synthetic. One per slot; a higher mark replaces a lower one.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Augment", fileName = "aug_")]
    public sealed class AugmentDefinition : DefinitionBase
    {
        [SerializeField] internal AugmentSlot slot;
        [SerializeField] internal AugmentKind kind = AugmentKind.Cyber;
        [SerializeField] internal Mark mark = Mark.MkI;
        [Min(0)] [SerializeField] internal int priceScrip = 450;
        [SerializeField] internal StatModifier[] modifiers = Array.Empty<StatModifier>();

        public AugmentSlot Slot => slot;
        public AugmentKind Kind => kind;
        public Mark Mark => mark;
        public int PriceScrip => priceScrip;
        public IReadOnlyList<StatModifier> Modifiers => modifiers ?? Array.Empty<StatModifier>();

        /// <summary>True when this augment would replace <paramref name="installed"/> as an upgrade.</summary>
        public bool Upgrades(AugmentDefinition installed) => installed == null || (installed.slot == slot && mark > installed.mark);

        protected override void ValidateDefinition(ValidationReport report)
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
