using System.Linq;
using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    public sealed class SimulationTests
    {
        // bay - corridor - crusher - nest; the crusher's machinery is loud (6).
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay")
                .Room("corridor", "Corridor")
                .Room("crusher", "Crusher bay", ambientNoise: 6)
                .Room("nest", "Nest")
                .Door("bay", "corridor")
                .Door("corridor", "crusher")
                .Door("crusher", "nest")
                .Build();

        private static Simulation NewSim(HulkGraph g) => new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });

        private static void RunSeconds(Simulation sim, int seconds)
        {
            for (var i = 0; i < seconds * SimTime.TicksPerSecond; i++)
            {
                sim.Step();
            }
        }

        [Test]
        public void ASilentShip_StaysDormant()
        {
            var sim = NewSim(Ship());
            RunSeconds(sim, 60);
            Assert.That(sim.Awareness.Value, Is.Zero);
            Assert.That(sim.Awareness.Stage, Is.EqualTo(HiveStage.Dormant));
            Assert.That(sim.Tick, Is.EqualTo(600));
        }

        [Test]
        public void WorkNearTheNest_WakesTheHive()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.AddNoiseSource(g.IdOf("crusher"), loudness: 9); // a cutter: 9 in the crusher, 7 reaches the nest

            RunSeconds(sim, 6);
            Assert.That(sim.Awareness.Value, Is.EqualTo(42f).Within(1e-3f), "7 heard for 6 s");
            Assert.That(sim.Awareness.Stage, Is.EqualTo(HiveStage.Restless));
        }

        [Test]
        public void WorkFarAway_IsNotHeard()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.AddNoiseSource(g.IdOf("bay"), loudness: 6); // fades out before the nest

            RunSeconds(sim, 30);
            Assert.That(sim.Awareness.Value, Is.Zero);
        }

        [Test]
        public void SealingADoor_SilencesTheWork()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.AddNoiseSource(g.IdOf("crusher"), loudness: 9);
            RunSeconds(sim, 2);
            var before = sim.Awareness.Value;

            sim.SetDoor(g.IdOf("crusher"), g.IdOf("nest"), DoorState.Sealed);
            RunSeconds(sim, 2);
            Assert.That(sim.Awareness.Value, Is.LessThan(before), "sealed: the gauge drains instead of filling");
        }

        [Test]
        public void TwoJobsInOneRoom_AreNotTwiceAsLoud()
        {
            var g = Ship();
            var one = NewSim(g);
            one.AddNoiseSource(g.IdOf("crusher"), 9);
            var two = NewSim(g);
            two.AddNoiseSource(g.IdOf("crusher"), 9);
            two.AddNoiseSource(g.IdOf("crusher"), 9);

            RunSeconds(one, 3);
            RunSeconds(two, 3);
            Assert.That(two.Awareness.Value, Is.EqualTo(one.Awareness.Value), "the hive hears the loudest sound, not a sum");
        }

        [Test]
        public void StoppingTheJob_LetsTheGaugeDrain()
        {
            var g = Ship();
            var sim = NewSim(g);
            var cutter = sim.AddNoiseSource(g.IdOf("crusher"), 9);
            RunSeconds(sim, 2);
            sim.RemoveNoiseSource(cutter);
            RunSeconds(sim, 2);
            Assert.That(sim.Awareness.Value, Is.EqualTo(10f).Within(1e-3f), "14 gained, 4 drained");
        }

        [Test]
        public void StageChanges_AreLogged_WithTheirTick()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.AddNoiseSource(g.IdOf("crusher"), 9);
            RunSeconds(sim, 20);

            var stages = sim.Events.Where(e => e.Kind == SimEventKind.HiveStageChanged).ToList();
            Assert.That(stages.Select(e => e.Stage), Is.EqualTo(new[] { HiveStage.Restless, HiveStage.Hunt }));
            Assert.That(stages[0].Tick, Is.EqualTo(58), "40 / 0.7 per tick = 57.1, so the 58th tick crosses it");
        }

        [Test]
        public void TheSameInputs_ReplayExactly()
        {
            string Run()
            {
                var g = Ship();
                var sim = NewSim(g);
                var cutter = sim.AddNoiseSource(g.IdOf("crusher"), 9);
                RunSeconds(sim, 3);
                sim.SetDoor(g.IdOf("crusher"), g.IdOf("nest"), DoorState.Closed);
                RunSeconds(sim, 5);
                sim.RemoveNoiseSource(cutter);
                RunSeconds(sim, 10);
                return string.Join("|", sim.Events.Select(e => e.ToString())) + "#" + sim.Awareness.Value;
            }

            Assert.That(Run(), Is.EqualTo(Run()));
        }
    }
}
