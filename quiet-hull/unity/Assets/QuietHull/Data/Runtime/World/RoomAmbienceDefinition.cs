using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// The ambient noise of a loud room, used for masking: inside it, a source's noise only
    /// counts above the ambient level, N = max(0, N0 − A).
    /// Steady: A = peak always. Pulsing and Burst: A = peak for <see cref="PeakSeconds"/> of every
    /// <see cref="PeriodSeconds"/>, base level otherwise (Burst is just a short, loud peak).
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Room Ambience", fileName = "amb_")]
    public sealed class RoomAmbienceDefinition : DefinitionBase
    {
        [SerializeField] internal AmbiencePattern pattern = AmbiencePattern.Steady;
        [Min(0f)] [SerializeField] internal float peakLevel = 6f;
        [Min(0f)] [SerializeField] internal float baseLevel;
        [Min(0f)] [SerializeField] internal float periodSeconds = 4f;
        [Min(0f)] [SerializeField] internal float peakSeconds = 1f;
        [Tooltip("Why the room is loud, shown in the room tooltip.")]
        [SerializeField] internal string causeText = string.Empty;

        public AmbiencePattern Pattern => pattern;
        public float PeakLevel => peakLevel;
        public float BaseLevel => baseLevel;
        public float PeriodSeconds => periodSeconds;
        public float PeakSeconds => peakSeconds;
        public string CauseText => causeText;

        /// <summary>Ambient level at <paramref name="timeSeconds"/> since the room's cycle started. Pure and thread-safe.</summary>
        public float AmbientLevelAt(float timeSeconds)
        {
            switch (pattern)
            {
                case AmbiencePattern.Silent:
                    return 0f;
                case AmbiencePattern.Steady:
                    return peakLevel;
                default:
                    if (periodSeconds <= 0f)
                    {
                        return peakLevel;
                    }

                    var phase = timeSeconds % periodSeconds;
                    if (phase < 0f)
                    {
                        phase += periodSeconds;
                    }

                    return phase < peakSeconds ? peakLevel : baseLevel;
            }
        }

        /// <summary>Whether a source of strength <paramref name="sourceNoise"/> is fully masked at that time.</summary>
        public bool IsMasked(float sourceNoise, float timeSeconds) => NoiseMath.Masked(sourceNoise, AmbientLevelAt(timeSeconds)) <= 0f;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NonNegative(this, nameof(peakLevel), peakLevel);
            if (pattern == AmbiencePattern.Pulsing || pattern == AmbiencePattern.Burst)
            {
                report.Positive(this, nameof(periodSeconds), periodSeconds);
                report.Positive(this, nameof(peakSeconds), peakSeconds);
                if (peakSeconds >= periodSeconds)
                {
                    report.Error(this, nameof(peakSeconds), "must be shorter than the period, or use Steady.");
                }

                if (baseLevel >= peakLevel)
                {
                    report.Warning(this, nameof(baseLevel), "is not below the peak, so the room never pulses.");
                }
            }

            if (pattern != AmbiencePattern.Silent && string.IsNullOrWhiteSpace(causeText))
            {
                report.Warning(this, nameof(causeText), "loud rooms should say why they're loud.");
            }
        }
    }
}
