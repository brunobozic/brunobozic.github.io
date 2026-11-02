using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// How the swarm moves: route costs for the shared flow fields, and each kind's speed, door forcing, breaching
    /// and whether it fits in ducts. All in ticks (10 per second). Immutable.
    /// </summary>
    public sealed class SwarmRules
    {
        private readonly int[] _moveTicks;
        private readonly int[] _forceDoorTicks;
        private readonly int[] _breachTicks;
        private readonly bool[] _usesDucts;

        public SwarmRules(int hopCost, int closedPenalty, int sealedPenalty, int[] moveTicks, int[] forceDoorTicks, int[] breachTicks, bool[] usesDucts)
        {
            var kinds = Enum.GetValues(typeof(AlienKind)).Length;
            if (hopCost < 1 || closedPenalty < 0 || sealedPenalty < closedPenalty)
            {
                throw new ArgumentException("route costs must be positive, and sealed must cost at least as much as closed.");
            }

            if (moveTicks?.Length != kinds || forceDoorTicks?.Length != kinds || breachTicks?.Length != kinds || usesDucts?.Length != kinds)
            {
                throw new ArgumentException($"per-kind tables need {kinds} entries.");
            }

            HopCost = hopCost;
            ClosedPenalty = closedPenalty;
            SealedPenalty = sealedPenalty;
            _moveTicks = (int[])moveTicks.Clone();
            _forceDoorTicks = (int[])forceDoorTicks.Clone();
            _breachTicks = (int[])breachTicks.Clone();
            _usesDucts = (bool[])usesDucts.Clone();
        }

        /// <summary>
        /// Crawler 1 s a room and fits ducts; Leaper fastest; Spitter slow; Husk slower; Bloat slowest but eats doors.
        /// Route costs: a hop 10, a closed door +30, a sealed door +300 (they break through if there's no other way).
        /// </summary>
        public static SwarmRules Default { get; } = new SwarmRules(
            hopCost: 10, closedPenalty: 30, sealedPenalty: 300,
            moveTicks: new[] { 10, 8, 14, 20, 30 },
            forceDoorTicks: new[] { 30, 20, 40, 30, 10 },
            breachTicks: new[] { 300, 250, 400, 300, 60 },
            usesDucts: new[] { true, false, false, false, false });

        public int HopCost { get; }
        public int ClosedPenalty { get; }
        public int SealedPenalty { get; }

        public int MoveTicks(AlienKind kind) => _moveTicks[(int)kind];
        public int ForceDoorTicks(AlienKind kind) => _forceDoorTicks[(int)kind];
        public int BreachTicks(AlienKind kind) => _breachTicks[(int)kind];
        public bool UsesDucts(AlienKind kind) => _usesDucts[(int)kind];

        /// <summary>Ticks for one kind to cross into the next room through a door in <paramref name="state"/> (or a duct).</summary>
        public int CrossingTicks(AlienKind kind, bool viaDuct, DoorState state)
        {
            var ticks = MoveTicks(kind);
            if (viaDuct)
            {
                return ticks;
            }

            return ticks + (state == DoorState.Closed ? ForceDoorTicks(kind) : state == DoorState.Sealed ? BreachTicks(kind) : 0);
        }

        /// <summary>The route cost of a link for the flow field (the same for every kind; speed differences are in the crossing).</summary>
        public int RouteCost(bool viaDuct, DoorState state) =>
            HopCost + (viaDuct ? 0 : state == DoorState.Closed ? ClosedPenalty : state == DoorState.Sealed ? SealedPenalty : 0);
    }
}
