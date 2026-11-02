using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>
    /// One run on one hulk: doors, emitters (sound, vibration, heat), the swarm, sensors, the hive's listeners and
    /// its Awareness, advanced in fixed ticks. Deterministic: the same inputs on the same tick always produce the
    /// same run. Event-driven: everything that happens is a <see cref="SimEvent"/> dispatched to handlers at the end
    /// of its tick. Allocation-free in steady state (buffers are reused; pools grow by doubling only).
    /// Not thread-safe by design: one simulation is owned by one thread; run many simulations in parallel.
    /// </summary>
    public sealed class Simulation
    {
        private const int InitialPool = 16;

        private readonly HulkGraph _graph;
        private readonly SimRules _rules;
        private readonly HiveListener[] _listeners;
        private readonly DoorStates _doors;
        private readonly List<SimEvent> _events = new List<SimEvent>(256);
        private readonly List<ISimEventHandler> _handlers = new List<ISimEventHandler>(4);
        private int _dispatched;

        // Emitters: a pooled table indexed by id (ids never reused within a run, so handles stay unambiguous).
        private int[] _emitterRoom = new int[InitialPool];
        private int[][] _emitterOutput = NewChannelArrays(InitialPool);
        private bool[] _emitterActive = new bool[InitialPool];
        private int _emitterCount;

        // Sensors (sentry pups in sensor mode).
        private int[] _sensorRoom = new int[InitialPool];
        private int[] _sensorRange = new int[InitialPool];
        private bool[] _sensorActive = new bool[InitialPool];
        private int _sensorCount;

        // Per-channel strongest arrival in every room, max over emitters. Recomputed only when something changes.
        private readonly int[][] _level;
        private readonly bool[] _covered;
        private readonly int[] _raw;
        private readonly int[] _queue;
        private readonly bool[] _queued;
        private readonly int[] _distance;
        private int _heardCache;
        private bool _signalsDirty;
        private bool _coverageDirty;

        public Simulation(HulkGraph graph, SimRules rules, IEnumerable<RoomId> hiveRooms, int swarmCapacity = 64)
            : this(graph, rules, ListenersFor(hiveRooms), swarmCapacity)
        {
        }

        public Simulation(HulkGraph graph, SimRules rules, IEnumerable<HiveListener> listeners, int swarmCapacity = 64)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _listeners = new List<HiveListener>(listeners ?? throw new ArgumentNullException(nameof(listeners))).ToArray();
            if (_listeners.Length == 0)
            {
                throw new ArgumentException("the hive must sense from at least one room.", nameof(listeners));
            }

            _doors = new DoorStates(graph);
            _level = NewChannelArrays(graph.RoomCount);
            _covered = new bool[graph.RoomCount];
            _raw = new int[graph.RoomCount];
            _queue = new int[graph.RoomCount];
            _queued = new bool[graph.RoomCount];
            _distance = new int[graph.RoomCount];
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

        // ---- Doors -----------------------------------------------------------------------------

        public DoorState Door(RoomId a, RoomId b) => _doors.Get(a, b);

        public void SetDoor(RoomId a, RoomId b, DoorState state)
        {
            if (_doors.Get(a, b) == state)
            {
                return;
            }

            _doors.Set(a, b, state);
            _signalsDirty = true;
        }

        // ---- Emitters --------------------------------------------------------------------------

        /// <summary>A sound-only emitter (kept for sound-only callers).</summary>
        public NoiseSourceId AddNoiseSource(RoomId room, int loudness)
        {
            if (loudness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(loudness), loudness, "loudness can't be negative.");
            }

            return AddEmitter(room, new Emission(loudness, 0, 0));
        }

        /// <summary>Something that gives off sound, vibration and heat: a working Joe, a cart, a fire.</summary>
        public NoiseSourceId AddEmitter(RoomId room, Emission emission)
        {
            if (_emitterCount == _emitterActive.Length)
            {
                var size = _emitterActive.Length * 2;
                Array.Resize(ref _emitterRoom, size);
                Array.Resize(ref _emitterActive, size);
                for (var c = 0; c < SenseChannels.Count; c++)
                {
                    Array.Resize(ref _emitterOutput[c], size);
                }
            }

            var id = _emitterCount++;
            _emitterRoom[id] = room.Index;
            _emitterActive[id] = true;
            for (var c = 0; c < SenseChannels.Count; c++)
            {
                _emitterOutput[c][id] = emission.Of((SenseChannel)c);
            }

            _signalsDirty = true;
            return new NoiseSourceId(id + 1);
        }

        /// <summary>Changes an emitter's sound only.</summary>
        public void SetLoudness(NoiseSourceId source, int loudness) => SetOutput(source, SenseChannel.Sound, loudness);

        public void SetOutput(NoiseSourceId source, SenseChannel channel, int strength)
        {
            var i = IndexOf(source);
            var value = Math.Max(0, strength);
            if (_emitterOutput[(int)channel][i] != value)
            {
                _emitterOutput[(int)channel][i] = value;
                _signalsDirty = true;
            }
        }

        public void RemoveNoiseSource(NoiseSourceId source)
        {
            _emitterActive[IndexOf(source)] = false;
            _signalsDirty = true;
        }

        // ---- Sensors ---------------------------------------------------------------------------

        /// <summary>A sentry pup in sensor mode: covers its room and every room within <paramref name="rangeHops"/>, through any door.</summary>
        public SensorId PlaceSensor(RoomId room, int rangeHops)
        {
            if (rangeHops < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rangeHops), rangeHops, "range can't be negative.");
            }

            if (_sensorCount == _sensorActive.Length)
            {
                var size = _sensorActive.Length * 2;
                Array.Resize(ref _sensorRoom, size);
                Array.Resize(ref _sensorRange, size);
                Array.Resize(ref _sensorActive, size);
            }

            var id = _sensorCount++;
            _sensorRoom[id] = room.Index;
            _sensorRange[id] = rangeHops;
            _sensorActive[id] = true;
            _coverageDirty = true;
            return new SensorId(id + 1);
        }

        public void RemoveSensor(SensorId sensor)
        {
            var i = sensor.Value - 1;
            if (i < 0 || i >= _sensorCount || !_sensorActive[i])
            {
                throw new ArgumentException($"{sensor} isn't placed.", nameof(sensor));
            }

            _sensorActive[i] = false;
            _coverageDirty = true;
        }

        /// <summary>Fills the map overlay: readings only where a sensor covers the room. Allocation-free.</summary>
        public void ReadOverlay(SensorOverlay overlay)
        {
            if (overlay == null) throw new ArgumentNullException(nameof(overlay));
            if (!ReferenceEquals(overlay.Graph, _graph)) throw new ArgumentException("the overlay was made for another hulk.", nameof(overlay));

            RefreshSignals();
            RefreshCoverage();
            for (var r = 0; r < _graph.RoomCount; r++)
            {
                var covered = _covered[r];
                overlay.Covered[r] = covered;
                for (var c = 0; c < SenseChannels.Count; c++)
                {
                    overlay.Arrival[c][r] = covered ? _level[c][r] : 0;
                }
            }
        }

        // ---- The tick --------------------------------------------------------------------------

        /// <summary>What the hive senses right now: the strongest weighted, unmasked signal at any listener.</summary>
        public int HeardByHive
        {
            get
            {
                RefreshSignals();
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
            if (i < 0 || i >= _emitterCount || !_emitterActive[i])
            {
                throw new ArgumentException($"{source} isn't active.", nameof(source));
            }

            return i;
        }

        private void RefreshSignals()
        {
            if (!_signalsDirty)
            {
                return;
            }

            for (var c = 0; c < SenseChannels.Count; c++)
            {
                var level = _level[c];
                Array.Clear(level, 0, level.Length);
                var rules = _rules.Senses.Of((SenseChannel)c);
                var output = _emitterOutput[c];
                for (var e = 0; e < _emitterCount; e++)
                {
                    if (!_emitterActive[e] || output[e] <= 0)
                    {
                        continue;
                    }

                    SenseField.SpreadInto(_graph, _doors, rules, new RoomId(_emitterRoom[e]), output[e], _raw, _queue, _queued);
                    for (var r = 0; r < level.Length; r++)
                    {
                        if (_raw[r] > level[r])
                        {
                            level[r] = _raw[r];
                        }
                    }
                }
            }

            var heard = 0;
            for (var l = 0; l < _listeners.Length; l++)
            {
                var listener = _listeners[l];
                var room = _graph.Room(listener.Room);
                for (var c = 0; c < SenseChannels.Count; c++)
                {
                    var channel = (SenseChannel)c;
                    var unmasked = Math.Max(0, _level[c][listener.Room.Index] - room.Ambient(channel));
                    heard = Math.Max(heard, unmasked * listener.Weights.Of(channel) / 100);
                }
            }

            _heardCache = heard;
            _signalsDirty = false;
        }

        private void RefreshCoverage()
        {
            if (!_coverageDirty)
            {
                return;
            }

            Array.Clear(_covered, 0, _covered.Length);
            for (var s = 0; s < _sensorCount; s++)
            {
                if (!_sensorActive[s])
                {
                    continue;
                }

                // Breadth-first by hops, through any door: pups read the hull, not the air.
                for (var r = 0; r < _distance.Length; r++) _distance[r] = -1;
                var start = _sensorRoom[s];
                _distance[start] = 0;
                _queue[0] = start;
                int head = 0, tail = 1;
                while (head < tail)
                {
                    var room = _queue[head++];
                    _covered[room] = true;
                    if (_distance[room] == _sensorRange[s])
                    {
                        continue;
                    }

                    var neighbours = _graph.Neighbours(new RoomId(room));
                    for (var i = 0; i < neighbours.Count; i++)
                    {
                        var next = neighbours[i].Index;
                        if (_distance[next] < 0)
                        {
                            _distance[next] = _distance[room] + 1;
                            _queue[tail++] = next;
                        }
                    }
                }
            }

            _coverageDirty = false;
        }

        private static int[][] NewChannelArrays(int length)
        {
            var arrays = new int[SenseChannels.Count][];
            for (var c = 0; c < arrays.Length; c++)
            {
                arrays[c] = new int[length];
            }

            return arrays;
        }

        private static IEnumerable<HiveListener> ListenersFor(IEnumerable<RoomId> rooms)
        {
            if (rooms == null) throw new ArgumentNullException(nameof(rooms));
            var list = new List<HiveListener>();
            foreach (var room in rooms)
            {
                list.Add(new HiveListener(room, SenseWeights.All));
            }

            return list;
        }
    }
}
