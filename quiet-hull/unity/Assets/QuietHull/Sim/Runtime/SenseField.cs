using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// The one propagation solver for every sense channel: each room keeps the strongest arrival over any path.
    /// Allocation-free form for the tick; a convenience form for tools and tests. Pure; safe from any thread.
    /// </summary>
    public static class SenseField
    {
        /// <summary>Convenience form: returns the raw strength per room (index by <see cref="RoomId.Index"/>).</summary>
        public static int[] Spread(HulkGraph graph, DoorStates doors, SenseRules rules, SenseChannel channel, RoomId source, int strength)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            var raw = new int[graph.RoomCount];
            SpreadInto(graph, doors, rules.Of(channel), source, strength, raw, new int[graph.RoomCount], new bool[graph.RoomCount]);
            return raw;
        }

        public static void SpreadInto(HulkGraph graph, DoorStates doors, ChannelRules rules, RoomId source, int strength,
            int[] raw, int[] queue, bool[] queued) =>
            SpreadCore(graph, doors, rules.LossPerHop, rules.ClosedDoorLoss, rules.SealedBlocks, source, strength, raw, queue, queued);

        internal static void SpreadCore(HulkGraph graph, DoorStates doors, int lossPerHop, int closedDoorLoss, bool sealedBlocks,
            RoomId source, int strength, int[] raw, int[] queue, bool[] queued)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (doors == null) throw new ArgumentNullException(nameof(doors));

            Array.Clear(raw, 0, graph.RoomCount);
            if (strength <= 0)
            {
                return;
            }

            var n = graph.RoomCount;
            var head = 0;
            raw[source.Index] = strength;
            queue[0] = source.Index;
            queued[source.Index] = true;
            var count = 1;

            while (count > 0)
            {
                var room = queue[head];
                head = (head + 1) % n;
                count--;
                queued[room] = false;

                var here = raw[room];
                var neighbours = graph.Neighbours(new RoomId(room));
                var doorIds = graph.DoorsOf(new RoomId(room));
                for (var i = 0; i < neighbours.Count; i++)
                {
                    var state = doors.ByIndex(doorIds[i]);
                    if (state == DoorState.Sealed && sealedBlocks)
                    {
                        continue;
                    }

                    var next = here - lossPerHop - (state != DoorState.Open ? closedDoorLoss : 0);
                    var target = neighbours[i].Index;
                    if (next > 0 && next > raw[target])
                    {
                        raw[target] = next;
                        if (!queued[target])
                        {
                            queue[(head + count) % n] = target;
                            queued[target] = true;
                            count++;
                        }
                    }
                }
            }
        }
    }
}
