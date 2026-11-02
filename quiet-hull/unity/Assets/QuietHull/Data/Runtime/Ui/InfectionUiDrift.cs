using System;

namespace QuietHull.Data
{
    /// <summary>
    /// How far Marlow's interface has drifted from company-clean to biomechanical, 0..1. Driven by the
    /// infection stage (I–V), plus a little for each splice and each Amplify or Accelerate step: the more
    /// alien he becomes, the less human his screens look. Pure; thread-safe.
    /// </summary>
    public static class InfectionUiDrift
    {
        public const int MinStage = 1;
        public const int MaxStage = 5;

        private static readonly float[] StageDrift = { 0f, 0.15f, 0.4f, 0.65f, 0.9f };

        public const float PerSplice = 0.03f;
        public const float PerLeverStep = 0.02f;

        /// <summary>The in-mission HUD drifts at most this much, so combat stays readable.</summary>
        public const float HudCap = 0.5f;

        public static float Drift(int infectionStage, int splices, int amplifyOrAccelerateSteps, UiDriftSetting setting)
        {
            if (infectionStage < MinStage || infectionStage > MaxStage)
            {
                throw new ArgumentOutOfRangeException(nameof(infectionStage), infectionStage, "infection stages run I..V.");
            }

            var raw = StageDrift[infectionStage - 1] + PerSplice * Math.Max(0, splices) + PerLeverStep * Math.Max(0, amplifyOrAccelerateSteps);
            raw = Math.Min(1f, raw);
            switch (setting)
            {
                case UiDriftSetting.Off:
                    return 0f;
                case UiDriftSetting.Reduced:
                    return raw * 0.5f;
                default:
                    return raw;
            }
        }

        public static float HudDrift(float drift) => Math.Min(HudCap, Math.Max(0f, drift));
    }
}
