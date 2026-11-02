using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A box of rounds for a deployable or weapon.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Ammunition", fileName = "itm_ammo_")]
    public sealed class AmmunitionDefinition : ItemDefinition
    {
        [Header("Ammunition")]
        [Min(1)] [SerializeField] internal int rounds = 500;
        [SerializeField] internal SentryDefinition compatibleSentry;

        public override ItemCategory Category => ItemCategory.Ammunition;
        public int Rounds => rounds;
        public SentryDefinition CompatibleSentry => compatibleSentry;

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(rounds), rounds);
            report.NotNull(this, nameof(compatibleSentry), compatibleSentry);
        }
    }
}
