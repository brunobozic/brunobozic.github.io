using System;

namespace QuietHull.Data
{
    /// <summary>Noise model helpers from the balance section of the design doc. Pure functions.</summary>
    public static class NoiseMath
    {
        /// <summary>Attenuation per edge crossed: open door, closed door, welded door.</summary>
        public const float OpenDoor = 0.5f;
        public const float ClosedDoor = 0.25f;
        public const float WeldedDoor = 0.1f;

        /// <summary>Noise that escapes masking in a room with ambient level A: max(0, N0 − A).</summary>
        public static float Masked(float sourceNoise, float ambientLevel) => Math.Max(0f, sourceNoise - Math.Max(0f, ambientLevel));
    }
}
