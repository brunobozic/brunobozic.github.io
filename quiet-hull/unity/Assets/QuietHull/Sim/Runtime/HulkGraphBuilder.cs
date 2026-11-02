using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>Builds a <see cref="HulkGraph"/>. Validates as it goes; <see cref="Build"/> returns an immutable graph.</summary>
    public sealed class HulkGraphBuilder
    {
        private readonly List<Room> _rooms = new List<Room>();
        private readonly Dictionary<string, RoomId> _byKey = new Dictionary<string, RoomId>(StringComparer.Ordinal);
        private readonly List<(string A, string B)> _doors = new List<(string A, string B)>();

        public HulkGraphBuilder Room(string key, string name, int ambientNoise = 0, int ambientVibration = 0, int ambientHeat = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("a room needs a key.", nameof(key));
            }

            if (_byKey.ContainsKey(key))
            {
                throw new ArgumentException($"room '{key}' already exists.", nameof(key));
            }

            if (ambientNoise < 0 || ambientVibration < 0 || ambientHeat < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ambientNoise), "ambient levels can't be negative.");
            }

            var id = new RoomId(_rooms.Count);
            _rooms.Add(new Room(id, key, name ?? key, ambientNoise, ambientVibration, ambientHeat));
            _byKey.Add(key, id);
            return this;
        }

        public HulkGraphBuilder Door(string a, string b)
        {
            _doors.Add((a, b));
            return this;
        }

        public HulkGraph Build()
        {
            var doors = new List<(int A, int B)>(_doors.Count);
            var seen = new HashSet<(int, int)>();
            foreach (var (a, b) in _doors)
            {
                if (!_byKey.TryGetValue(a ?? string.Empty, out var ra) || !_byKey.TryGetValue(b ?? string.Empty, out var rb))
                {
                    throw new ArgumentException($"door {a}–{b} leads to a room that doesn't exist.");
                }

                if (ra == rb)
                {
                    throw new ArgumentException($"door {a}–{b} connects a room to itself.");
                }

                var pair = ra.Index < rb.Index ? (ra.Index, rb.Index) : (rb.Index, ra.Index);
                if (!seen.Add(pair))
                {
                    throw new ArgumentException($"door {a}–{b} is defined twice.");
                }

                doors.Add((ra.Index, rb.Index));
            }

            return new HulkGraph(_rooms.ToArray(), new Dictionary<string, RoomId>(_byKey, StringComparer.Ordinal), doors);
        }
    }
}
