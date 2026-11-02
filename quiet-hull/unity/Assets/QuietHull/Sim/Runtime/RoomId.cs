using System;

namespace QuietHull.Sim
{
    /// <summary>A room's index in its <see cref="HulkGraph"/>. Cheap to copy and compare.</summary>
    public readonly struct RoomId : IEquatable<RoomId>
    {
        public RoomId(int index) => Index = index;

        public int Index { get; }

        public bool Equals(RoomId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is RoomId other && Equals(other);
        public override int GetHashCode() => Index;
        public static bool operator ==(RoomId a, RoomId b) => a.Index == b.Index;
        public static bool operator !=(RoomId a, RoomId b) => a.Index != b.Index;
        public override string ToString() => $"Room#{Index}";
    }
}
