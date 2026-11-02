using System.Diagnostics;
using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>500 aliens on a 200-room hulk: no allocations, inside the frame budget.</summary>
    [Category("Performance")]
    public sealed class SwarmPerformanceTests
    {
        private const int Width = 20, Height = 10; // 200 rooms

        private static HulkGraph Grid()
        {
            var b = new HulkGraphBuilder();
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                b.Room($"r{x}_{y}", $"R{x},{y}", ambientNoise: (x * 7 + y * 3) % 5);
            }

            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                if (x + 1 < Width) b.Door($"r{x}_{y}", $"r{x + 1}_{y}");
                if (y + 1 < Height) b.Door($"r{x}_{y}", $"r{x}_{y + 1}");
                if (x + 3 < Width && y % 3 == 0) b.Duct($"r{x}_{y}", $"r{x + 3}_{y}");
            }

            return b.Build();
        }

        private static Simulation Swarming(out HulkGraph g)
        {
            g = Grid();
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf($"r{Width - 1}_{Height - 1}") }, swarmCapacity: 512);
            sim.SetShuttle(g.IdOf("r0_0"));
            sim.ForceStage(HiveStage.Hunt);
            var kinds = new[] { AlienKind.Crawler, AlienKind.Leaper, AlienKind.Spitter, AlienKind.Husk, AlienKind.Bloat };
            for (var i = 0; i < 500; i++)
            {
                sim.Swarm.Spawn(kinds[i % kinds.Length], g.IdOf($"r{Width - 1 - i % 5}_{Height - 1 - i % 3}"), 40);
            }

            sim.AddNoiseSource(g.IdOf("r2_2"), 8);
            return sim;
        }

#if NETCOREAPP
        [Test]
        public void FiveHundredAliens_AllocateNothing_WhileTheyHunt()
        {
            var sim = Swarming(out var g);
            for (var i = 0; i < 20; i++) sim.Step();

            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 600; i++)
            {
                if (i % 100 == 0) sim.SetDoor(g.IdOf("r1_0"), g.IdOf("r2_0"), i % 200 == 0 ? DoorState.Sealed : DoorState.Open);
                sim.Step();
            }

            Assert.That(System.GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
        }
#endif

        [Test]
        public void FiveHundredAliens_TickInsideTheBudget()
        {
            var sim = Swarming(out var g);
            for (var i = 0; i < 50; i++) sim.Step(); // warm up the JIT

            var watch = Stopwatch.StartNew();
            const int ticks = 1000;
            for (var i = 0; i < ticks; i++)
            {
                if (i % 100 == 0) sim.SetDoor(g.IdOf("r1_0"), g.IdOf("r2_0"), i % 200 == 0 ? DoorState.Sealed : DoorState.Open);
                sim.Step();
            }

            watch.Stop();
            var msPerTick = watch.Elapsed.TotalMilliseconds / ticks;
            TestContext.WriteLine($"{msPerTick:0.000} ms per tick with 500 aliens on 200 rooms");
            // 10 ticks per second: 2 ms per tick is 2% of a core. Generous so it holds on slow CI machines in Debug.
            Assert.That(msPerTick, Is.LessThan(2.0));
        }
    }
}
