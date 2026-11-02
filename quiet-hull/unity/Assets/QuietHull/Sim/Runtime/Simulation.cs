using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>
    /// One run on one hulk: doors, continuous noise sources, the hive's listening rooms and its Awareness,
    /// advanced in fixed ticks. Deterministic: the same inputs on the same tick always produce the same run.
    /// Not thread-safe by design: one simulation is owned by one thread (the game loop or a balance-runner worker);
    /// run many simulations in parallel instead of sharing one.
    /// </summary>
    public sealed class Simulation
    {
        private readonly HulkGraph _graph;
        private readonly SimRules _rules;
        private readonly RoomId[] _hiveRooms;
        private readonly DoorStates _doors;
        private readonly SortedDictionary<int, (RoomId Room, int Loudness)> _sources = new SortedDictionary<int, (RoomId, int)>();
        private readonly List<SimEvent> _events = new List<SimEvent>();
        private int _nextSourceId = 1;
        private int _heardCache;
        private bool _dirty;

        public Simulation(HulkGraph graph, SimRules rules, IEnumerable<RoomId> hiveRooms)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _hiveRooms = new List<RoomId>(hiveRooms ?? throw new ArgumentNullException(nameof(hiveRooms))).ToArray();
            if (_hiveRooms.Length == 0)
            {
                throw new ArgumentException("the hive must listen from at least one room.", nameof(hiveRooms));
            }

            _doors = new DoorStates(graph);
            Awareness = new AwarenessGauge(rules.Awareness);
            Awareness.StageChanged += (from, to) => _events.Add(new SimEvent(Tick, SimEventKind.HiveStageChanged, to));
        }

        public HulkGraph Graph => _graph;
        public int Tick { get; private set; }
        public AwarenessGauge Awareness { get; }
        public IReadOnlyList<SimEvent> Events => _events;

        public DoorState Door(RoomId a, RoomId b) => _doors.Get(a, b);

        public void SetDoor(RoomId a, RoomId b, DoorState state)
        {
            _doors.Set(a, b, state);
            _dirty = true;
        }

        public NoiseSourceId AddNoiseSource(RoomId room, int loudness)
        {
            if (loudness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(loudness), loudness, "loudness can't be negative.");
            }

            var id = _nextSourceId++;
            _sources.Add(id, (room, loudness));
            _dirty = true;
            return new NoiseSourceId(id);
        }

        public void RemoveNoiseSource(NoiseSourceId source)
        {
            if (!_sources.Remove(source.Value))
            {
                throw new ArgumentException($"{source} isn't active.", nameof(source));
            }

            _dirty = true;
        }

        /// <summary>What the hive hears right now: the loudest sound reaching any of its rooms, after masking.</summary>
        public int HeardByHive
        {
            get
            {
                if (_dirty)
                {
                    _heardCache = ComputeHeard();
                    _dirty = false;
                }

                return _heardCache;
            }
        }

        /// <summary>Advances one tick.</summary>
        public void Step()
        {
            Tick++;
            Awareness.Tick(HeardByHive);
        }

        private int ComputeHeard()
        {
            var loudest = 0;
            foreach (var source in _sources.Values)
            {
                var field = NoisePropagation.Spread(_graph, _doors, _rules.Noise, source.Room, source.Loudness);
                for (var i = 0; i < _hiveRooms.Length; i++)
                {
                    loudest = Math.Max(loudest, field.HeardAt(_hiveRooms[i]));
                }
            }

            return loudest;
        }
    }
}
