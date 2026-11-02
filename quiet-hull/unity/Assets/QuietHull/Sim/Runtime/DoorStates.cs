using System;

namespace QuietHull.Sim
{
    /// <summary>The state of every door in one run. Owned by the simulation; single-threaded like the tick.</summary>
    public sealed class DoorStates
    {
        private readonly HulkGraph _graph;
        private readonly DoorState[] _states;

        public DoorStates(HulkGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _states = new DoorState[graph.DoorCount];
        }

        public DoorState Get(RoomId a, RoomId b) => _states[_graph.DoorBetween(a, b)];

        public void Set(RoomId a, RoomId b, DoorState state) => _states[_graph.DoorBetween(a, b)] = state;

        internal DoorState ByIndex(int door) => _states[door];
    }
}
