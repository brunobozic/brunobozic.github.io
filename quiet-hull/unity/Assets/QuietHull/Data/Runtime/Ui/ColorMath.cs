using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// Colour maths for the drifting UI theme: perceptual blending in OKLab (so a blend between two palettes
    /// never passes through muddy greys) and WCAG contrast checks. Pure functions; thread-safe.
    /// </summary>
    public static class ColorMath
    {
        /// <summary>WCAG AA for body text.</summary>
        public const float TextContrastMinimum = 4.5f;

        /// <summary>WCAG AA for large text and UI marks (danger, warnings, frames that carry meaning).</summary>
        public const float MarkContrastMinimum = 3f;

        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                throw new ArgumentException("empty colour.", nameof(hex));
            }

            var s = hex[0] == '#' ? hex.Substring(1) : hex;
            if (s.Length != 6)
            {
                throw new FormatException($"'{hex}' is not #RRGGBB.");
            }

            var value = Convert.ToInt32(s, 16);
            return new Color(((value >> 16) & 0xFF) / 255f, ((value >> 8) & 0xFF) / 255f, (value & 0xFF) / 255f, 1f);
        }

        /// <summary>Perceptual blend from <paramref name="a"/> to <paramref name="b"/> (t in 0..1), through OKLab.</summary>
        public static Color Blend(Color a, Color b, float t)
        {
            t = t < 0f ? 0f : t > 1f ? 1f : t;
            var la = ToOkLab(a);
            var lb = ToOkLab(b);
            var mixed = FromOkLab(la.L + (lb.L - la.L) * t, la.A + (lb.A - la.A) * t, la.B + (lb.B - la.B) * t);
            mixed.a = a.a + (b.a - a.a) * t;
            return mixed;
        }

        /// <summary>WCAG relative luminance.</summary>
        public static float Luminance(Color c) => 0.2126f * ToLinear(c.r) + 0.7152f * ToLinear(c.g) + 0.0722f * ToLinear(c.b);

        /// <summary>WCAG contrast ratio, 1..21.</summary>
        public static float Contrast(Color a, Color b)
        {
            var la = Luminance(a);
            var lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05f) / (Math.Min(la, lb) + 0.05f);
        }

        private readonly struct OkLab
        {
            public OkLab(float l, float a, float b)
            {
                L = l;
                A = a;
                B = b;
            }

            public float L { get; }
            public float A { get; }
            public float B { get; }
        }

        private static OkLab ToOkLab(Color c)
        {
            var r = ToLinear(c.r);
            var g = ToLinear(c.g);
            var b = ToLinear(c.b);
            var l = Cbrt(0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b);
            var m = Cbrt(0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b);
            var s = Cbrt(0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b);
            return new OkLab(
                0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s,
                1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s,
                0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s);
        }

        private static Color FromOkLab(float L, float A, float B)
        {
            var l = L + 0.3963377774f * A + 0.2158037573f * B;
            var m = L - 0.1055613458f * A - 0.0638541728f * B;
            var s = L - 0.0894841775f * A - 1.2914855480f * B;
            l = l * l * l;
            m = m * m * m;
            s = s * s * s;
            return new Color(
                ToSrgb(4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s),
                ToSrgb(-1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s),
                ToSrgb(-0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s),
                1f);
        }

        private static float Cbrt(float x) => (float)Math.Pow(Math.Max(0f, x), 1.0 / 3.0);

        private static float ToLinear(float c) => c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4);

        private static float ToSrgb(float c)
        {
            c = c < 0f ? 0f : c > 1f ? 1f : c;
            return c <= 0.0031308f ? c * 12.92f : 1.055f * (float)Math.Pow(c, 1 / 2.4) - 0.055f;
        }
    }
}
