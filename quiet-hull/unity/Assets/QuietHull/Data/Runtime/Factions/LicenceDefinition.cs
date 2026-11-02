using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A licence or permit the player can hold (Lane Authority military licence, Vashti-Kell industrial licence).</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Factions/Licence", fileName = "lic_")]
    public sealed class LicenceDefinition : DefinitionBase
    {
        [SerializeField] internal FactionDefinition issuedBy;
        [Tooltip("Lane Authority scans are less likely while it's held.")]
        [Range(0f, 1f)] [SerializeField] internal float scanChanceReduction;

        public FactionDefinition IssuedBy => issuedBy;
        public float ScanChanceReduction => scanChanceReduction;

        protected override void ValidateDefinition(ValidationReport report) => report.NotNull(this, nameof(issuedBy), issuedBy);
    }
}
