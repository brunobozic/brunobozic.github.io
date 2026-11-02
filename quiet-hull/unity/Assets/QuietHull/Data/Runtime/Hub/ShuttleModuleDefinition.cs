using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A module fitted to the shuttle (the Cargo Cradle in mission 2).</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Hub/Shuttle Module", fileName = "mod_")]
    public sealed class ShuttleModuleDefinition : DefinitionBase
    {
        [Min(1)] [SerializeField] internal int moduleSlotsUsed = 1;
        [Tooltip("Extra hold slots for normal cargo.")]
        [Min(0)] [SerializeField] internal int extraHoldSlots;
        [Tooltip("Oversize loads (a cryo pod) it can carry. The shuttle can't carry any without one.")]
        [Min(0)] [SerializeField] internal int oversizeLoadCapacity;
        [Min(0)] [SerializeField] internal int priceScrip;

        [Header("Hidden compartment")]
        [Tooltip("Contraband items it can hide from a Lane Authority scan.")]
        [Min(0)] [SerializeField] internal int hiddenItemSlots;
        [Tooltip("Units it can hide powered down (a synth fitted with Shadow parts).")]
        [Min(0)] [SerializeField] internal int hiddenUnitBerths;
        [Tooltip("Chance a scan misses what's hidden inside.")]
        [Range(0f, 1f)] [SerializeField] internal float scanEvasion;
        [SerializeField] internal Legality legality = Legality.Legal;

        public int ModuleSlotsUsed => moduleSlotsUsed;
        public int ExtraHoldSlots => extraHoldSlots;
        public int OversizeLoadCapacity => oversizeLoadCapacity;
        public int PriceScrip => priceScrip;
        public int HiddenItemSlots => hiddenItemSlots;
        public int HiddenUnitBerths => hiddenUnitBerths;
        public float ScanEvasion => scanEvasion;
        public Legality Legality => legality;
        public bool IsHiddenCompartment => hiddenItemSlots > 0 || hiddenUnitBerths > 0;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (extraHoldSlots == 0 && oversizeLoadCapacity == 0 && !IsHiddenCompartment)
            {
                report.Error(this, nameof(extraHoldSlots), "the module must add hold slots, oversize capacity or hidden space.");
            }

            if (IsHiddenCompartment && !(scanEvasion > 0f))
            {
                report.Error(this, nameof(scanEvasion), "a hidden compartment must have a chance to fool scans.");
            }
        }
    }
}
