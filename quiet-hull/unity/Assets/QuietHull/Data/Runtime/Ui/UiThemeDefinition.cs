using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// The commander's interface across the infection: five palettes from Stage I (company-industrial: charcoal,
    /// amber, sharp frames) to Stage V (biomechanical: blue-black, bone, cold chrome, ribbed frames, a heartbeat).
    /// <see cref="Evaluate"/> blends them by drift. Company terminals never use it: they stay corporate-clean, so the
    /// contrast tells the player it's Marlow's view that is changing. Read-only at runtime; thread-safe.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/UI/UI Theme", fileName = "ui_theme_")]
    public sealed class UiThemeDefinition : DefinitionBase
    {
        public const int StageCount = 5;

        /// <summary>Validation samples every step along the drift.</summary>
        public const float ValidationStep = 0.05f;

        [Tooltip("Stage I..V, at drift 0, 0.25, 0.5, 0.75, 1.")]
        [SerializeField] internal UiPalette[] stages = Array.Empty<UiPalette>();

        public IReadOnlyList<UiPalette> Stages => stages ?? Array.Empty<UiPalette>();

        /// <summary>The palette at <paramref name="drift"/> (0..1), blended perceptually between the two nearest stages.</summary>
        public UiPalette Evaluate(float drift)
        {
            var list = Stages;
            if (list.Count == 0)
            {
                throw new InvalidOperationException($"{Id} has no palettes.");
            }

            drift = drift < 0f ? 0f : drift > 1f ? 1f : drift;
            var position = drift * (list.Count - 1);
            var index = (int)Math.Floor(position);
            if (index >= list.Count - 1)
            {
                return list[list.Count - 1];
            }

            return UiPalette.Blend(list[index], list[index + 1], position - index);
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (Stages.Count != StageCount)
            {
                report.Error(this, nameof(stages), $"needs {StageCount} palettes (Stage I..V).");
                return;
            }

            for (var drift = 0f; drift <= 1.0001f; drift += ValidationStep)
            {
                var p = Evaluate(drift);
                Check(report, drift, "text on background", p.textPrimary, p.background, ColorMath.TextContrastMinimum);
                Check(report, drift, "text on surface", p.textPrimary, p.surface, ColorMath.TextContrastMinimum);
                Check(report, drift, "muted text on background", p.textMuted, p.background, ColorMath.TextContrastMinimum);
                Check(report, drift, "danger on background", p.danger, p.background, ColorMath.MarkContrastMinimum);
                Check(report, drift, "warning on background", p.warning, p.background, ColorMath.MarkContrastMinimum);
                Check(report, drift, "accent on background", p.accent, p.background, ColorMath.MarkContrastMinimum);
            }
        }

        private void Check(ValidationReport report, float drift, string what, Color fg, Color bg, float minimum)
        {
            var ratio = ColorMath.Contrast(fg, bg);
            if (ratio < minimum)
            {
                report.Error(this, nameof(stages), $"drift {drift:0.00}: {what} contrast {ratio:0.0}:1 is under {minimum:0.0}:1.");
            }
        }
    }
}
