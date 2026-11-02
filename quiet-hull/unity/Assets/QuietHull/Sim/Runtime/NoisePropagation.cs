using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// Spreads a sound from its room through the doors. Each room keeps the loudest arrival over any path;
    /// open doors cost <see cref="NoiseRules.LossPerHop"/>, closed doors cost more, sealed doors stop it.
    /// Pure: touches nothing shared, so it's safe from any thread.
    /// </summary>
    public static class NoisePropagation
    {
        /// <summary>Convenience form: allocates its buffers and returns the field. The tick uses <see cref="SpreadInto"/>.</summary>
        public static NoiseField Spread(HulkGraph graph, DoorStates doors, NoiseRules rules, RoomId source, int loudness)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (doors == null) throw new ArgumentNullException(nameof(doors));
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            var raw = new int[graph.RoomCount];
            SpreadInto(graph, doors, rules, source, loudness, raw, new int[graph.RoomCount], new bool[graph.RoomCount]);
            return new NoiseField(graph, raw);
        }

        /// <summary>
        /// The allocation-free form for the tick: writes loudness per room into <paramref name="raw"/>
        /// (length RoomCount), using <paramref name="queue"/> and <paramref name="queued"/> (length RoomCount) as
        /// scratch. A room is queued at most once at a time, so the ring buffer never overflows.
        /// </summary>
        public static void SpreadInto(HulkGraph graph, DoorStates doors, NoiseRules rules, RoomId source, int loudness,
            int[] raw, int[] queue, bool[] queued)
        {
            Array.Clear(raw, 0, graph.RoomCount);
            if (loudness <= 0)
            {
                return;
            }

            var n = graph.RoomCount;
            var head = 0;
            raw[source.Index] = loudness;
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
                    if (state == DoorState.Sealed)
                    {
                        continue;
                    }

                    var next = here - rules.LossPerHop - (state == DoorState.Closed ? rules.ClosedDoorLoss : 0);
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
