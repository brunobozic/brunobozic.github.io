using System;

namespace QuietHull.Sim
{
    /// <summary>Handle to a continuous sound in a simulation (a Joe cutting, a cart rolling).</summary>
    public readonly struct NoiseSourceId : IEquatable<NoiseSourceId>
    {
        internal NoiseSourceId(int value) => Value = value;

        public int Value { get; }

        public bool Equals(NoiseSourceId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is NoiseSourceId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Noise#{Value}";
    }
}
