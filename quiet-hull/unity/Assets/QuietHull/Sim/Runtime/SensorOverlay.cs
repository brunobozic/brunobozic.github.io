using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// A reusable buffer the map UI reads every frame. Create one per hulk; <see cref="Simulation.ReadOverlay"/>
    /// fills it without allocating.
    /// </summary>
    public sealed class SensorOverlay
    {
        internal readonly bool[] Covered;
        internal readonly int[][] Arrival;
        private readonly HulkGraph _graph;

        public SensorOverlay(HulkGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Covered = new bool[graph.RoomCount];
            Arrival = new int[SenseChannels.Count][];
            for (var c = 0; c < SenseChannels.Count; c++)
            {
                Arrival[c] = new int[graph.RoomCount];
            }
        }

        public HulkGraph Graph => _graph;

        public RoomReading At(RoomId room)
        {
            var i = room.Index;
            if (!Covered[i])
            {
                return default;
            }

            var r = _graph.Room(room);
            return new RoomReading(true,
                Arrival[(int)SenseChannel.Sound][i], Arrival[(int)SenseChannel.Vibration][i], Arrival[(int)SenseChannel.Heat][i],
                r.AmbientNoise, r.AmbientVibration, r.AmbientHeat);
        }
    }
}
