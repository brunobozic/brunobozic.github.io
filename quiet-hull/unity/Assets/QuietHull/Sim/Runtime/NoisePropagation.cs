using System;
using System.Collections.Generic;

namespace QuietHull.Sim
{
    /// <summary>
    /// Spreads a sound from its room through the doors. Each room keeps the loudest arrival over any path;
    /// open doors cost <see cref="NoiseRules.LossPerHop"/>, closed doors cost more, sealed doors stop it.
    /// Pure: allocates its result and touches nothing shared, so it's safe from any thread.
    /// </summary>
    public static class NoisePropagation
    {
        public static NoiseField Spread(HulkGraph graph, DoorStates doors, NoiseRules rules, RoomId source, int loudness)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (doors == null) throw new ArgumentNullException(nameof(doors));
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            var raw = new int[graph.RoomCount];
            if (loudness <= 0)
            {
                return new NoiseField(graph, raw);
            }

            raw[source.Index] = loudness;
            var queue = new Queue<RoomId>();
            queue.Enqueue(source);
            while (queue.Count > 0)
            {
                var room = queue.Dequeue();
                var here = raw[room.Index];
                var neighbours = graph.Neighbours(room);
                var doorIds = graph.DoorsOf(room);
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
                        queue.Enqueue(neighbours[i]);
                    }
                }
            }

            return new NoiseField(graph, raw);
        }
    }
}
