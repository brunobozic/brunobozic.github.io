using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// One look of the commander's interface: colour tokens plus the biomechanical effects the UI shader reads
    /// (grain, a heartbeat pulse, a wet sheen, how far frames have grown organic). The theme blends between these.
    /// </summary>
    [Serializable]
    public struct UiPalette
    {
        public Color background;
        public Color surface;
        public Color frame;
        public Color textPrimary;
        public Color textMuted;
        public Color accent;
        public Color danger;
        public Color warning;

        [Tooltip("Film grain over panels, 0..1.")]
        [Range(0f, 1f)] public float grain;
        [Tooltip("Panels breathe at a slow heartbeat, 0..1 amplitude.")]
        [Range(0f, 1f)] public float pulse;
        [Tooltip("Wet, oily highlights on frames, 0..1.")]
        [Range(0f, 1f)] public float sheen;
        [Tooltip("0 = sharp industrial frames; 1 = ribbed, vertebral, biomechanical frames (cross-faded sprite sets).")]
        [Range(0f, 1f)] public float organicFrames;

        public static UiPalette Blend(UiPalette a, UiPalette b, float t)
        {
            t = t < 0f ? 0f : t > 1f ? 1f : t;
            return new UiPalette
            {
                background = ColorMath.Blend(a.background, b.background, t),
                surface = ColorMath.Blend(a.surface, b.surface, t),
                frame = ColorMath.Blend(a.frame, b.frame, t),
                textPrimary = ColorMath.Blend(a.textPrimary, b.textPrimary, t),
                textMuted = ColorMath.Blend(a.textMuted, b.textMuted, t),
                accent = ColorMath.Blend(a.accent, b.accent, t),
                danger = ColorMath.Blend(a.danger, b.danger, t),
                warning = ColorMath.Blend(a.warning, b.warning, t),
                grain = a.grain + (b.grain - a.grain) * t,
                pulse = a.pulse + (b.pulse - a.pulse) * t,
                sheen = a.sheen + (b.sheen - a.sheen) * t,
                organicFrames = a.organicFrames + (b.organicFrames - a.organicFrames) * t
            };
        }
    }
}
