using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A hulk the player can come back to (the Tremolino and its T2 fabricator). Each return costs
    /// docking; the hulk keeps eroding, so an outpost lasts a limited number of visits.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Outpost", fileName = "outpost_")]
    public sealed class OutpostDefinition : DefinitionBase
    {
        [SerializeField] internal string hulkName = string.Empty;
        [SerializeField] internal WorkstationDefinition workstation;
        [SerializeField] internal SunExposureDefinition sunExposure;
        [SerializeField] internal BeltErosionDefinition belt;
        [Tooltip("Return visits before the hulk breaks up (0 = unlimited).")]
        [Min(0)] [SerializeField] internal int visitBudget = 6;
        [Tooltip("Docking consumables each return costs.")]
        [Min(0)] [SerializeField] internal int dockingConsumables = 5;
        [Tooltip("Hive stage the hulk is at on each return.")]
        [SerializeField] internal HiveStage returnStage = HiveStage.Restless;

        public string HulkName => hulkName;
        public WorkstationDefinition Workstation => workstation;
        public SunExposureDefinition SunExposure => sunExposure;
        public BeltErosionDefinition Belt => belt;
        public int VisitBudget => visitBudget;
        public int DockingConsumables => dockingConsumables;
        public HiveStage ReturnStage => returnStage;

        public bool CanVisit(int visitsSoFar) => visitBudget == 0 || visitsSoFar < visitBudget;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(workstation), workstation);
            if (string.IsNullOrWhiteSpace(hulkName))
            {
                report.Error(this, nameof(hulkName), "is empty.");
            }
        }
    }
}
