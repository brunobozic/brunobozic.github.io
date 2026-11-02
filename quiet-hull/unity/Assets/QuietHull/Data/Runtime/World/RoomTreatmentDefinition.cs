using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A treatment installed in a room (acoustic insulation). It multiplies what leaves the room,
    /// so loud work inside can be "Contained": below the hive's hearing threshold outside.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Room Treatment", fileName = "rt_")]
    public sealed class RoomTreatmentDefinition : DefinitionBase
    {
        [SerializeField] internal MaterialDefinition panel;
        [Min(1)] [SerializeField] internal int panelsRequired = 8;
        [Min(0.1f)] [SerializeField] internal float installSecondsPerPanel = 6f;

        [Range(0f, 1f)] [SerializeField] internal float soundMultiplier = 0.1f;
        [Range(0f, 1f)] [SerializeField] internal float vibrationMultiplier = 0.5f;

        [Tooltip("Noise leaving the room at or below this counts as Contained on the HUD.")]
        [Min(0f)] [SerializeField] internal float containedThreshold = 1f;

        public MaterialDefinition Panel => panel;
        public int PanelsRequired => panelsRequired;
        public float InstallSecondsPerPanel => installSecondsPerPanel;
        public float SoundMultiplier => soundMultiplier;
        public float VibrationMultiplier => vibrationMultiplier;
        public float ContainedThreshold => containedThreshold;

        /// <summary>Total install time with <paramref name="workers"/> working in parallel.</summary>
        public float InstallSeconds(int workers) =>
            workers <= 0 ? float.PositiveInfinity : Mathf.Ceil((float)panelsRequired / workers) * installSecondsPerPanel;

        public float SoundLeaving(float sourceNoise) => sourceNoise * soundMultiplier;

        public float VibrationLeaving(float sourceVibration) => sourceVibration * vibrationMultiplier;

        public bool IsContained(float sourceNoise, float sourceVibration) =>
            SoundLeaving(sourceNoise) <= containedThreshold && VibrationLeaving(sourceVibration) <= containedThreshold * 3f;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(panel), panel);
            if (soundMultiplier >= 1f && vibrationMultiplier >= 1f)
            {
                report.Error(this, nameof(soundMultiplier), "the treatment doesn't reduce anything.");
            }
        }
    }
}
