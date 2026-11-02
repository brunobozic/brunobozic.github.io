using System.Collections.Generic;
using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>Event-driven output and the zero-allocation tick.</summary>
    public sealed class EventsAndAllocationTests
    {
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay").Room("corridor", "Corridor").Room("crusher", "Crusher", ambientNoise: 6).Room("nest", "Nest")
                .Door("bay", "corridor").Door("corridor", "crusher").Door("crusher", "nest")
                .Build();

        private sealed class Recorder : ISimEventHandler
        {
            public readonly List<SimEvent> Seen = new List<SimEvent>();
            public void Handle(in SimEvent e) => Seen.Add(e);
        }

        private sealed class Telemetry : IRunTelemetry
        {
            public int Ticks;
            public int LastHeard;
            public float LastAwareness;
            public void OnTick(int tick, int heardByHive, float awareness)
            {
                Ticks++;
                LastHeard = heardByHive;
                LastAwareness = awareness;
            }
        }

        [Test]
        public void Handlers_ReceiveEvents_AtTheEndOfTheirTick()
        {
            var g = Ship();
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });
            var recorder = new Recorder();
            sim.Subscribe(recorder);
            sim.AddNoiseSource(g.IdOf("crusher"), 9);

            for (var i = 0; i < 200; i++) sim.Step();
            Assert.That(recorder.Seen.Count, Is.EqualTo(2));
            Assert.That(recorder.Seen[0].Stage, Is.EqualTo(HiveStage.Restless));
            Assert.That(recorder.Seen[0].Tick, Is.EqualTo(58));
        }

        [Test]
        public void Telemetry_SeesEveryTick()
        {
            var g = Ship();
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });
            var telemetry = new Telemetry();
            sim.Telemetry = telemetry;
            sim.AddNoiseSource(g.IdOf("crusher"), 9);

            for (var i = 0; i < 30; i++) sim.Step();
            Assert.That(telemetry.Ticks, Is.EqualTo(30));
            Assert.That(telemetry.LastHeard, Is.EqualTo(7));
            Assert.That(telemetry.LastAwareness, Is.EqualTo(21f).Within(1e-3f));
        }

#if NETCOREAPP
        [Test]
        public void TheTick_AllocatesNothing_EvenWhenDoorsAndSoundsChange()
        {
            var g = Ship();
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });
            var cutter = sim.AddNoiseSource(g.IdOf("crusher"), 9);
            var swarm = sim.Swarm;
            for (var i = 0; i < 64; i++) swarm.Spawn(AlienKind.Crawler, g.IdOf("nest"), 40);
            for (var i = 0; i < 10; i++) sim.Step(); // warm up

            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 1000; i++)
            {
                if (i % 50 == 0)
                {
                    sim.SetDoor(g.IdOf("crusher"), g.IdOf("nest"), i % 100 == 0 ? DoorState.Closed : DoorState.Open);
                    sim.SetLoudness(cutter, 8 + i % 3);
                }

                sim.Step();
            }

            var allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero, $"{allocated} bytes allocated in 1,000 ticks");
        }

        [Test]
        public void SpawnAndDespawn_WithinCapacity_AllocateNothing()
        {
            var swarm = new SwarmStore(capacity: 256);
            var handles = new AlienHandle[256];
            for (var i = 0; i < 256; i++) handles[i] = swarm.Spawn(AlienKind.Crawler, new RoomId(0), 40);
            for (var i = 0; i < 256; i++) swarm.Despawn(handles[i]);

            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (var round = 0; round < 10; round++)
            {
                for (var i = 0; i < 256; i++) handles[i] = swarm.Spawn(AlienKind.Leaper, new RoomId(1), 60);
                for (var i = 0; i < 256; i++) swarm.Despawn(handles[i]);
            }

            Assert.That(System.GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
        }
#endif
    }
}
