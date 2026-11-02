using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>
    /// A shared route map: one Dijkstra from the goal rooms gives every room its distance to the nearest goal.
    /// Every alien of a mobility class reads the same map and steps to the neighbour that gets it closer, so
    /// routing 500 aliens costs the same as routing one. Recomputed only when doors or goals change.
    /// Allocation-free after construction (binary heap over preallocated arrays).
    /// </summary>
    public sealed class FlowField
    {
        public const int Unreachable = int.MaxValue;

        private readonly HulkGraph _graph;
        private readonly SwarmRules _rules;
        private readonly bool _useDucts;
        private readonly int[] _distance;
        private readonly int[] _heapRoom;
        private readonly int[] _heapKey;
        private int _heapCount;

        public FlowField(HulkGraph graph, SwarmRules rules, bool useDucts)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _useDucts = useDucts;
            _distance = new int[graph.RoomCount];
            // Lazy-deletion heap: at most one entry per room as a start, plus one per relaxation of each link direction.
            var capacity = graph.RoomCount + 2 * (graph.DoorCount + graph.DuctCount) + 1;
            _heapRoom = new int[capacity];
            _heapKey = new int[capacity];
            for (var i = 0; i < _distance.Length; i++) _distance[i] = Unreachable;
        }

        public int DistanceOf(RoomId room) => _distance[room.Index];

        public void Compute(DoorStates doors, IReadOnlyList<RoomId> goals)
        {
            if (goals == null) throw new ArgumentNullException(nameof(goals));
            Begin();
            for (var i = 0; i < goals.Count; i++) Seed(goals[i].Index);
            Run(doors);
        }

        internal void Compute(DoorStates doors, int[] goals, int goalCount)
        {
            Begin();
            for (var i = 0; i < goalCount; i++) Seed(goals[i]);
            Run(doors);
        }

        /// <summary>
        /// The best next room from <paramref name="room"/> toward the goal: the link with the lowest
        /// distance-plus-cost (which equals this room's own distance); the lowest room index breaks ties, so it's deterministic.
        /// </summary>
        public FlowStep NextStep(RoomId room, DoorStates doors)
        {
            var here = _distance[room.Index];
            if (here == 0 || here == Unreachable)
            {
                return default;
            }

            var bestScore = Unreachable;
            var best = default(FlowStep);
            var neighbours = _graph.Neighbours(room);
            var doorIds = _graph.DoorsOf(room);
            for (var i = 0; i < neighbours.Count; i++)
            {
                var state = doors.ByIndex(doorIds[i]);
                Consider(neighbours[i], false, state, ref bestScore, ref best);
            }

            if (_useDucts)
            {
                var ducts = _graph.DuctNeighbours(room);
                for (var i = 0; i < ducts.Count; i++)
                {
                    Consider(ducts[i], true, DoorState.Open, ref bestScore, ref best);
                }
            }

            return best;
        }

        private void Consider(RoomId next, bool viaDuct, DoorState state, ref int bestScore, ref FlowStep best)
        {
            var d = _distance[next.Index];
            if (d == Unreachable)
            {
                return;
            }

            var score = d + _rules.RouteCost(viaDuct, state);
            if (score < bestScore || (score == bestScore && next.Index < best.Room.Index))
            {
                bestScore = score;
                best = new FlowStep(next, viaDuct, state);
            }
        }

        private void Begin()
        {
            for (var i = 0; i < _distance.Length; i++) _distance[i] = Unreachable;
            _heapCount = 0;
        }

        private void Seed(int room)
        {
            if (_distance[room] == 0) return;
            _distance[room] = 0;
            Push(room, 0);
        }

        private void Run(DoorStates doors)
        {
            while (_heapCount > 0)
            {
                Pop(out var room, out var key);
                if (key > _distance[room]) continue; // stale entry

                var id = new RoomId(room);
                var neighbours = _graph.Neighbours(id);
                var doorIds = _graph.DoorsOf(id);
                for (var i = 0; i < neighbours.Count; i++)
                {
                    Relax(neighbours[i].Index, key + _rules.RouteCost(false, doors.ByIndex(doorIds[i])));
                }

                if (_useDucts)
                {
                    var ducts = _graph.DuctNeighbours(id);
                    for (var i = 0; i < ducts.Count; i++)
                    {
                        Relax(ducts[i].Index, key + _rules.RouteCost(true, DoorState.Open));
                    }
                }
            }
        }

        private void Relax(int room, int distance)
        {
            if (distance < _distance[room])
            {
                _distance[room] = distance;
                Push(room, distance);
            }
        }

        private void Push(int room, int key)
        {
            var i = _heapCount++;
            while (i > 0)
            {
                var parent = (i - 1) >> 1;
                if (_heapKey[parent] <= key) break;
                _heapRoom[i] = _heapRoom[parent];
                _heapKey[i] = _heapKey[parent];
                i = parent;
            }

            _heapRoom[i] = room;
            _heapKey[i] = key;
        }

        private void Pop(out int room, out int key)
        {
            room = _heapRoom[0];
            key = _heapKey[0];
            var lastRoom = _heapRoom[--_heapCount];
            var lastKey = _heapKey[_heapCount];
            var i = 0;
            while (true)
            {
                var child = 2 * i + 1;
                if (child >= _heapCount) break;
                if (child + 1 < _heapCount && _heapKey[child + 1] < _heapKey[child]) child++;
                if (_heapKey[child] >= lastKey) break;
                _heapRoom[i] = _heapRoom[child];
                _heapKey[i] = _heapKey[child];
                i = child;
            }

            if (_heapCount > 0)
            {
                _heapRoom[i] = lastRoom;
                _heapKey[i] = lastKey;
            }
        }
    }
}
