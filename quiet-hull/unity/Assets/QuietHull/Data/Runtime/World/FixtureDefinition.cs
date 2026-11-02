using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// Something bolted into the ship that a worker detaches and hauls (a cryo pod in its mount).
    /// Detaching is slow and loud; do it in a treated room.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Fixture", fileName = "fx_")]
    public sealed class FixtureDefinition : DefinitionBase
    {
        [SerializeField] internal CargoDefinition yields;
        [SerializeField] internal ToolActionKind requiredAction = ToolActionKind.DetachFixture;
        [Min(0.1f)] [SerializeField] internal float detachSeconds = 90f;
        [Tooltip("Noise N0 while detaching.")]
        [Min(0f)] [SerializeField] internal float noise = 9f;
        [Min(0f)] [SerializeField] internal float vibration = 6f;
        [Tooltip("Condition lost if detaching is interrupted (yanked out).")]
        [Min(0f)] [SerializeField] internal float interruptConditionLoss = 25f;
        [Min(1)] [SerializeField] internal int placedCount = 1;
        [Tooltip("Scrip paid the moment it's detached (a salvage bonus, on top of the contract).")]
        [Min(0)] [SerializeField] internal int salvageBonusScrip;

        public CargoDefinition Yields => yields;
        public ToolActionKind RequiredAction => requiredAction;
        public float DetachSeconds => detachSeconds;
        public float Noise => noise;
        public float Vibration => vibration;
        public float InterruptConditionLoss => interruptConditionLoss;
        public int PlacedCount => placedCount;
        public int SalvageBonusScrip => salvageBonusScrip;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(yields), yields);
            report.Positive(this, nameof(detachSeconds), detachSeconds);
            report.Positive(this, nameof(placedCount), placedCount);
        }
    }
}
