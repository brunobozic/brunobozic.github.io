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
        private readonly List<(string A, string B)> _ducts = new List<(string A, string B)>();

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

        /// <summary>A crawlspace between two rooms, used only by alien kinds that fit (Crawlers).</summary>
        public HulkGraphBuilder Duct(string a, string b)
        {
            _ducts.Add((a, b));
            return this;
        }

        public HulkGraph Build()
        {
            var doors = Resolve(_doors, "door");
            var ducts = Resolve(_ducts, "duct");
            return new HulkGraph(_rooms.ToArray(), new Dictionary<string, RoomId>(_byKey, StringComparer.Ordinal), doors, ducts);
        }

        private List<(int A, int B)> Resolve(List<(string A, string B)> links, string what)
        {
            var resolved = new List<(int A, int B)>(links.Count);
            var seen = new HashSet<(int, int)>();
            foreach (var (a, b) in links)
            {
                if (!_byKey.TryGetValue(a ?? string.Empty, out var ra) || !_byKey.TryGetValue(b ?? string.Empty, out var rb))
                {
                    throw new ArgumentException($"{what} {a}–{b} leads to a room that doesn't exist.");
                }

                if (ra == rb)
                {
                    throw new ArgumentException($"{what} {a}–{b} connects a room to itself.");
                }

                var pair = ra.Index < rb.Index ? (ra.Index, rb.Index) : (rb.Index, ra.Index);
                if (!seen.Add(pair))
                {
                    throw new ArgumentException($"{what} {a}–{b} is defined twice.");
                }

                resolved.Add((ra.Index, rb.Index));
            }

            return resolved;
        }
    }
}
