using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// The swarm, stored the way an ECS stores components: struct-of-arrays, densely packed, no object per alien.
    /// Slots are pooled (a free list) and guarded by generations; despawning swaps the last alien into the hole,
    /// so systems iterate 0..Count over tight arrays. Only plain value types, so the same layout ports directly
    /// to Unity DOTS (Burst jobs over NativeArrays) if the swarm ever needs it. Spawning and despawning allocate
    /// nothing until capacity has to grow (it doubles). Single-threaded, owned by its simulation.
    /// </summary>
    public sealed class SwarmStore
    {
        // Sparse: per slot.
        private int[] _generation;
        private int[] _denseOf;
        private int[] _free;
        private int _freeCount;
        private int _highWater;

        // Dense: per living alien, packed 0..Count-1.
        private int[] _slotOf;
        private AlienKind[] _kind;
        private int[] _room;
        private int[] _health;
        private int[] _next;      // room being moved into, or -1
        private int[] _progress;  // ticks left on the current crossing

        public SwarmStore(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "capacity must be at least 1.");
            }

            _generation = new int[capacity];
            _denseOf = new int[capacity];
            _free = new int[capacity];
            _slotOf = new int[capacity];
            _kind = new AlienKind[capacity];
            _room = new int[capacity];
            _health = new int[capacity];
            _next = new int[capacity];
            _progress = new int[capacity];
        }

        public int Count { get; private set; }
        public int Capacity => _generation.Length;

        public AlienHandle Spawn(AlienKind kind, RoomId room, int health)
        {
            if (health <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(health), health, "an alien spawns alive.");
            }

            int slot;
            if (_freeCount > 0)
            {
                slot = _free[--_freeCount];
            }
            else
            {
                if (_highWater == Capacity)
                {
                    Grow();
                }

                slot = _highWater++;
            }

            var dense = Count++;
            _denseOf[slot] = dense;
            _slotOf[dense] = slot;
            _kind[dense] = kind;
            _room[dense] = room.Index;
            _health[dense] = health;
            _next[dense] = -1;
            _progress[dense] = 0;
            return new AlienHandle(slot, _generation[slot]);
        }

        public bool IsAlive(AlienHandle handle) =>
            handle.Slot >= 0 && handle.Slot < _highWater && _generation[handle.Slot] == handle.Generation && _denseOf[handle.Slot] >= 0 &&
            _denseOf[handle.Slot] < Count && _slotOf[_denseOf[handle.Slot]] == handle.Slot;

        public bool Despawn(AlienHandle handle)
        {
            if (!IsAlive(handle))
            {
                return false;
            }

            var dense = _denseOf[handle.Slot];
            var last = Count - 1;
            if (dense != last)
            {
                var movedSlot = _slotOf[last];
                _slotOf[dense] = movedSlot;
                _kind[dense] = _kind[last];
                _room[dense] = _room[last];
                _health[dense] = _health[last];
                _next[dense] = _next[last];
                _progress[dense] = _progress[last];
                _denseOf[movedSlot] = dense;
            }

            Count--;
            _denseOf[handle.Slot] = -1;
            _generation[handle.Slot]++;
            _free[_freeCount++] = handle.Slot;
            return true;
        }

        /// <summary>Deals damage; returns true (and despawns) if it was lethal.</summary>
        public bool Damage(AlienHandle handle, int amount)
        {
            if (!IsAlive(handle))
            {
                return false;
            }

            var dense = _denseOf[handle.Slot];
            _health[dense] -= Math.Max(0, amount);
            if (_health[dense] > 0)
            {
                return false;
            }

            Despawn(handle);
            return true;
        }

        public AlienKind KindOf(AlienHandle handle) => _kind[DenseOf(handle)];
        public RoomId RoomOf(AlienHandle handle) => new RoomId(_room[DenseOf(handle)]);
        public int HealthOf(AlienHandle handle) => _health[DenseOf(handle)];

        public void MoveTo(AlienHandle handle, RoomId room) => _room[DenseOf(handle)] = room.Index;

        // Dense access for systems: iterate i in 0..Count-1.
        public AlienHandle DenseHandle(int i) => new AlienHandle(_slotOf[i], _generation[_slotOf[i]]);
        public AlienKind DenseKind(int i) => _kind[i];
        public RoomId DenseRoom(int i) => new RoomId(_room[i]);
        public int DenseHealth(int i) => _health[i];

        /// <summary>True while the alien is crossing into another room.</summary>
        public bool IsMoving(AlienHandle handle) => _next[DenseOf(handle)] >= 0;

        // Movement state for the swarm systems (dense index).
        internal int NextRoom(int dense) => _next[dense];
        internal int Progress(int dense) => _progress[dense];

        internal void BeginCrossing(int dense, int room, int ticks)
        {
            _next[dense] = room;
            _progress[dense] = ticks;
        }

        /// <summary>Advances a crossing by one tick; returns true when the alien arrives (and moves it).</summary>
        internal bool AdvanceCrossing(int dense)
        {
            if (_next[dense] < 0)
            {
                return false;
            }

            if (--_progress[dense] > 0)
            {
                return false;
            }

            _room[dense] = _next[dense];
            _next[dense] = -1;
            _progress[dense] = 0;
            return true;
        }

        public int CountIn(RoomId room)
        {
            var n = 0;
            for (var i = 0; i < Count; i++)
            {
                if (_room[i] == room.Index)
                {
                    n++;
                }
            }

            return n;
        }

        private int DenseOf(AlienHandle handle) =>
            IsAlive(handle) ? _denseOf[handle.Slot] : throw new ArgumentException($"{handle} is not alive.", nameof(handle));

        private void Grow()
        {
            var size = Capacity * 2;
            Array.Resize(ref _generation, size);
            Array.Resize(ref _denseOf, size);
            Array.Resize(ref _free, size);
            Array.Resize(ref _slotOf, size);
            Array.Resize(ref _kind, size);
            Array.Resize(ref _room, size);
            Array.Resize(ref _health, size);
            Array.Resize(ref _next, size);
            Array.Resize(ref _progress, size);
        }
    }
}
