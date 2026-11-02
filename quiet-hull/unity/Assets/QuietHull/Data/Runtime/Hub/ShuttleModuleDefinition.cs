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

        public int ModuleSlotsUsed => moduleSlotsUsed;
        public int ExtraHoldSlots => extraHoldSlots;
        public int OversizeLoadCapacity => oversizeLoadCapacity;
        public int PriceScrip => priceScrip;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (extraHoldSlots == 0 && oversizeLoadCapacity == 0)
            {
                report.Error(this, nameof(extraHoldSlots), "the module must add hold slots or oversize capacity.");
            }
        }
    }
}
