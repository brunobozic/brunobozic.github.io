using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>
    /// One run on one hulk: doors, continuous noise sources, the swarm, the hive's listening rooms and its
    /// Awareness, advanced in fixed ticks. Deterministic: the same inputs on the same tick always produce the
    /// same run. Event-driven: everything that happens is a <see cref="SimEvent"/> dispatched to handlers at the
    /// end of its tick. Allocation-free in steady state (buffers are reused; pools grow by doubling only).
    /// Not thread-safe by design: one simulation is owned by one thread; run many simulations in parallel.
    /// </summary>
    public sealed class Simulation
    {
        private const int InitialSources = 16;

        private readonly HulkGraph _graph;
        private readonly SimRules _rules;
        private readonly RoomId[] _hiveRooms;
        private readonly DoorStates _doors;
        private readonly List<SimEvent> _events = new List<SimEvent>(256);
        private readonly List<ISimEventHandler> _handlers = new List<ISimEventHandler>(4);
        private int _dispatched;

        // Noise sources: a pooled table indexed by id (ids are never reused within a run, so handles stay unambiguous).
        private int[] _sourceRoom = new int[InitialSources];
        private int[] _sourceLoudness = new int[InitialSources];
        private bool[] _sourceActive = new bool[InitialSources];
        private int _sourceCount;

        // Scratch for the noise solver, reused every recompute.
        private readonly int[] _raw;
        private readonly int[] _queue;
        private readonly bool[] _queued;
        private int _heardCache;
        private bool _dirty;

        public Simulation(HulkGraph graph, SimRules rules, IEnumerable<RoomId> hiveRooms, int swarmCapacity = 64)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _hiveRooms = new List<RoomId>(hiveRooms ?? throw new ArgumentNullException(nameof(hiveRooms))).ToArray();
            if (_hiveRooms.Length == 0)
            {
                throw new ArgumentException("the hive must listen from at least one room.", nameof(hiveRooms));
            }

            _doors = new DoorStates(graph);
            _raw = new int[graph.RoomCount];
            _queue = new int[graph.RoomCount];
            _queued = new bool[graph.RoomCount];
            Swarm = new SwarmStore(swarmCapacity);
            Awareness = new AwarenessGauge(rules.Awareness);
            Awareness.StageChanged += OnStageChanged;
        }

        public HulkGraph Graph => _graph;
        public int Tick { get; private set; }
        public AwarenessGauge Awareness { get; }
        public SwarmStore Swarm { get; }
        public IReadOnlyList<SimEvent> Events => _events;

        /// <summary>Optional per-tick measurements (the fun gates). Null = off.</summary>
        public IRunTelemetry Telemetry { get; set; }

        public void Subscribe(ISimEventHandler handler) => _handlers.Add(handler ?? throw new ArgumentNullException(nameof(handler)));

        public bool Unsubscribe(ISimEventHandler handler) => _handlers.Remove(handler);

        public DoorState Door(RoomId a, RoomId b) => _doors.Get(a, b);

        public void SetDoor(RoomId a, RoomId b, DoorState state)
        {
            if (_doors.Get(a, b) == state)
            {
                return;
            }

            _doors.Set(a, b, state);
            _dirty = true;
        }

        public NoiseSourceId AddNoiseSource(RoomId room, int loudness)
        {
            if (loudness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(loudness), loudness, "loudness can't be negative.");
            }

            if (_sourceCount == _sourceActive.Length)
            {
                var size = _sourceActive.Length * 2;
                Array.Resize(ref _sourceRoom, size);
                Array.Resize(ref _sourceLoudness, size);
                Array.Resize(ref _sourceActive, size);
            }

            var id = _sourceCount++;
            _sourceRoom[id] = room.Index;
            _sourceLoudness[id] = loudness;
            _sourceActive[id] = true;
            _dirty = true;
            return new NoiseSourceId(id + 1);
        }

        public void SetLoudness(NoiseSourceId source, int loudness)
        {
            var i = IndexOf(source);
            if (_sourceLoudness[i] != loudness)
            {
                _sourceLoudness[i] = Math.Max(0, loudness);
                _dirty = true;
            }
        }

        public void RemoveNoiseSource(NoiseSourceId source)
        {
            _sourceActive[IndexOf(source)] = false;
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

        /// <summary>Advances one tick, then dispatches the tick's events.</summary>
        public void Step()
        {
            Tick++;
            var heard = HeardByHive;
            Awareness.Tick(heard);
            Telemetry?.OnTick(Tick, heard, Awareness.Value);
            Dispatch();
        }

        private void Dispatch()
        {
            for (; _dispatched < _events.Count; _dispatched++)
            {
                var e = _events[_dispatched];
                for (var h = 0; h < _handlers.Count; h++)
                {
                    _handlers[h].Handle(in e);
                }
            }
        }

        private void OnStageChanged(HiveStage from, HiveStage to) => _events.Add(new SimEvent(Tick, SimEventKind.HiveStageChanged, to));

        private int IndexOf(NoiseSourceId source)
        {
            var i = source.Value - 1;
            if (i < 0 || i >= _sourceCount || !_sourceActive[i])
            {
                throw new ArgumentException($"{source} isn't active.", nameof(source));
            }

            return i;
        }

        private int ComputeHeard()
        {
            var loudest = 0;
            for (var s = 0; s < _sourceCount; s++)
            {
                if (!_sourceActive[s])
                {
                    continue;
                }

                NoisePropagation.SpreadInto(_graph, _doors, _rules.Noise, new RoomId(_sourceRoom[s]), _sourceLoudness[s], _raw, _queue, _queued);
                for (var i = 0; i < _hiveRooms.Length; i++)
                {
                    var room = _hiveRooms[i];
                    loudest = Math.Max(loudest, Math.Max(0, _raw[room.Index] - _graph.Room(room).AmbientNoise));
                }
            }

            return loudest;
        }
    }
}
