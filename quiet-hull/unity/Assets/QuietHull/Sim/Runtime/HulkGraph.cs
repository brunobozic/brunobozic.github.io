using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>
    /// A hulk's layout: rooms and the doors between them. Immutable once built, so one graph can be shared by
    /// any number of simulations and threads. Doors are numbered; per-run state (open, closed, sealed) lives in
    /// <see cref="DoorStates"/>.
    /// </summary>
    public sealed class HulkGraph
    {
        private readonly Room[] _rooms;
        private readonly Dictionary<string, RoomId> _byKey;
        private readonly RoomId[][] _neighbours;
        private readonly int[][] _doorOf;
        private readonly Dictionary<long, int> _doorIndex;

        internal HulkGraph(Room[] rooms, Dictionary<string, RoomId> byKey, List<(int A, int B)> doors)
        {
            _rooms = rooms;
            _byKey = byKey;
            DoorCount = doors.Count;
            _doorIndex = new Dictionary<long, int>(doors.Count);

            var neighbours = new List<RoomId>[rooms.Length];
            var doorOf = new List<int>[rooms.Length];
            for (var i = 0; i < rooms.Length; i++)
            {
                neighbours[i] = new List<RoomId>(4);
                doorOf[i] = new List<int>(4);
            }

            for (var d = 0; d < doors.Count; d++)
            {
                var (a, b) = doors[d];
                neighbours[a].Add(new RoomId(b));
                doorOf[a].Add(d);
                neighbours[b].Add(new RoomId(a));
                doorOf[b].Add(d);
                _doorIndex[Key(a, b)] = d;
            }

            _neighbours = new RoomId[rooms.Length][];
            _doorOf = new int[rooms.Length][];
            for (var i = 0; i < rooms.Length; i++)
            {
                _neighbours[i] = neighbours[i].ToArray();
                _doorOf[i] = doorOf[i].ToArray();
            }
        }

        public int RoomCount => _rooms.Length;
        public int DoorCount { get; }

        public Room Room(RoomId id) => _rooms[id.Index];

        public RoomId IdOf(string key)
        {
            if (key != null && _byKey.TryGetValue(key, out var id))
            {
                return id;
            }

            throw new ArgumentException($"no room '{key}' in this hulk.", nameof(key));
        }

        public IReadOnlyList<RoomId> Neighbours(RoomId id) => _neighbours[id.Index];

        /// <summary>The door index for each neighbour, parallel to <see cref="Neighbours"/>.</summary>
        internal int[] DoorsOf(RoomId id) => _doorOf[id.Index];

        /// <summary>The door between two adjacent rooms.</summary>
        public int DoorBetween(RoomId a, RoomId b)
        {
            if (_doorIndex.TryGetValue(Key(a.Index, b.Index), out var door))
            {
                return door;
            }

            throw new ArgumentException($"{Room(a).Key} and {Room(b).Key} aren't connected.");
        }

        private static long Key(int a, int b) => a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
    }
}
