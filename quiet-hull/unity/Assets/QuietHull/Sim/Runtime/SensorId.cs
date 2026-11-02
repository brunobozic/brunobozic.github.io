using System;

namespace QuietHull.Sim
{
    /// <summary>Handle to a placed sensor (a sentry pup in sensor mode).</summary>
    public readonly struct SensorId : IEquatable<SensorId>
    {
        internal SensorId(int value) => Value = value;

        public int Value { get; }

        public bool Equals(SensorId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SensorId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Sensor#{Value}";
    }
}
