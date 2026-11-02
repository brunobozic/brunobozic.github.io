using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// Sound on its own: kept as the simple entry point for sound-only tools and tests. The solver is
    /// <see cref="SenseField"/>, shared by every channel.
    /// </summary>
    public static class NoisePropagation
    {
        public static NoiseField Spread(HulkGraph graph, DoorStates doors, NoiseRules rules, RoomId source, int loudness)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            var raw = new int[graph.RoomCount];
            SenseField.SpreadCore(graph, doors, rules.LossPerHop, rules.ClosedDoorLoss, true, source, loudness,
                raw, new int[graph.RoomCount], new bool[graph.RoomCount]);
            return new NoiseField(graph, raw);
        }
    }
}
