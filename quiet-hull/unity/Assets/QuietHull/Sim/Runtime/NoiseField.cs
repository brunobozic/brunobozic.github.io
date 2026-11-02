using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// How loud one sound is in every room of the hulk. <see cref="RawAt"/> is what arrives;
    /// <see cref="HeardAt"/> is what's left after the room's own ambient noise masks it.
    /// </summary>
    public sealed class NoiseField
    {
        private readonly HulkGraph _graph;
        private readonly int[] _raw;

        internal NoiseField(HulkGraph graph, int[] raw)
        {
            _graph = graph;
            _raw = raw;
        }

        public int RawAt(RoomId room) => _raw[room.Index];

        public int HeardAt(RoomId room) => Math.Max(0, _raw[room.Index] - _graph.Room(room).AmbientNoise);
    }
}
