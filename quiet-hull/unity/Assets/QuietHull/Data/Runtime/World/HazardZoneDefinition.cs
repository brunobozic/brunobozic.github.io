using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A room hazard that a unit switches off at an interaction point: a dark section
    /// (restore power at a junction) or a coolant flood (shut a valve). The cleared state can
    /// leave a milder residue (the junction stays at Cold 1).
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Hazard Zone", fileName = "hz_")]
    public sealed class HazardZoneDefinition : DefinitionBase
    {
        [SerializeField] internal HazardKind kind = HazardKind.Dark;
        [Range(0, 3)] [SerializeField] internal int intensity = 3;

        [Header("Active")]
        [Range(0.05f, 1f)] [SerializeField] internal float moveSpeedMultiplier = 1f;
        [Min(0f)] [SerializeField] internal float damagePerSecond;
        [Tooltip("Dark only: how far a unit sees with its lamp on and off.")]
        [Min(0f)] [SerializeField] internal float visionWithLamp = 8f;
        [Min(0f)] [SerializeField] internal float visionWithoutLamp = 2f;
        [Tooltip("Dark only: aliens spot a lit lamp from this multiple of their normal vision range.")]
        [Min(1f)] [SerializeField] internal float lampSpottedRangeMultiplier = 2f;

        [Header("Clearing")]
        [Tooltip("Interaction point id placed in the level (junction box, valve).")]
        [SerializeField] internal string clearedAtInteractionId = string.Empty;
        [Min(0f)] [SerializeField] internal float clearSeconds = 5f;
        [Min(0f)] [SerializeField] internal float clearNoise = 4f;
        [Tooltip("Optional key item needed to reach the interaction point.")]
        [SerializeField] internal KeyItemDefinition requiresKey;

        [Header("After clearing")]
        [Range(0, 3)] [SerializeField] internal int residualIntensity;
        [Range(0.05f, 1f)] [SerializeField] internal float residualMoveSpeedMultiplier = 1f;
        [Min(0f)] [SerializeField] internal float residualDamagePerSecond;

        public HazardKind Kind => kind;
        public int Intensity => intensity;
        public float MoveSpeedMultiplier => moveSpeedMultiplier;
        public float DamagePerSecond => damagePerSecond;
        public float VisionWithLamp => visionWithLamp;
        public float VisionWithoutLamp => visionWithoutLamp;
        public float LampSpottedRangeMultiplier => lampSpottedRangeMultiplier;
        public string ClearedAtInteractionId => clearedAtInteractionId;
        public float ClearSeconds => clearSeconds;
        public float ClearNoise => clearNoise;
        public KeyItemDefinition RequiresKey => requiresKey;
        public int ResidualIntensity => residualIntensity;
        public float ResidualMoveSpeedMultiplier => residualMoveSpeedMultiplier;
        public float ResidualDamagePerSecond => residualDamagePerSecond;

        /// <summary>Seconds until a unit with <paramref name="health"/> is disabled while standing in the active hazard.</summary>
        public float SecondsToDisable(float health) => damagePerSecond > 0f ? health / damagePerSecond : float.PositiveInfinity;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(clearedAtInteractionId))
            {
                report.Error(this, nameof(clearedAtInteractionId), "is empty: the player must be able to switch the hazard off.");
            }

            report.Positive(this, nameof(clearSeconds), clearSeconds);

            if (residualIntensity > intensity)
            {
                report.Error(this, nameof(residualIntensity), "is higher than the active intensity.");
            }

            if (residualDamagePerSecond > damagePerSecond)
            {
                report.Error(this, nameof(residualDamagePerSecond), "is higher than the active damage.");
            }

            if (kind == HazardKind.Dark)
            {
                report.Positive(this, nameof(visionWithLamp), visionWithLamp);
                if (visionWithoutLamp >= visionWithLamp)
                {
                    report.Warning(this, nameof(visionWithoutLamp), "is not below the lamp's range, so the lamp is pointless.");
                }
            }
        }
    }
}
