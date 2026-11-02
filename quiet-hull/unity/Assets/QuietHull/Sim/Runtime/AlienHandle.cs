using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// A reference to one alien in a <see cref="SwarmStore"/>: its pooled slot plus the slot's generation.
    /// When the alien dies and the slot is reused, the generation changes, so old handles stop working
    /// instead of silently pointing at a different alien.
    /// </summary>
    public readonly struct AlienHandle : IEquatable<AlienHandle>
    {
        internal AlienHandle(int slot, int generation)
        {
            Slot = slot;
            Generation = generation;
        }

        public int Slot { get; }
        public int Generation { get; }

        public bool Equals(AlienHandle other) => Slot == other.Slot && Generation == other.Generation;
        public override bool Equals(object obj) => obj is AlienHandle other && Equals(other);
        public override int GetHashCode() => (Slot * 397) ^ Generation;
        public override string ToString() => $"Alien#{Slot}.{Generation}";
    }
}
