using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>The swarm moves by stage: Dormant stays, Restless investigates noise, Hunt goes for the shuttle.</summary>
    public sealed class SwarmMovementTests
    {
        // bay - a - b - nest, with a duct a ~ nest.
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay").Room("a", "A").Room("b", "B").Room("nest", "Nest")
                .Door("bay", "a").Door("a", "b").Door("b", "nest")
                .Duct("a", "nest")
                .Build();

        private static Simulation Hunting(HulkGraph g)
        {
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });
            sim.SetShuttle(g.IdOf("bay"));
            sim.ForceStage(HiveStage.Hunt);
            return sim;
        }

        private static void RunTicks(Simulation sim, int ticks)
        {
            for (var i = 0; i < ticks; i++) sim.Step();
        }

        [Test]
        public void Dormant_AliensStayPut()
        {
            var g = Ship();
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });
            sim.SetShuttle(g.IdOf("bay"));
            var husk = sim.Swarm.Spawn(AlienKind.Husk, g.IdOf("nest"), 60);
            RunTicks(sim, 300);
            Assert.That(sim.Swarm.RoomOf(husk), Is.EqualTo(g.IdOf("nest")));
        }

        [Test]
        public void Hunting_AWalkerFollowsTheDoors_AtItsOwnSpeed()
        {
            var g = Ship();
            var sim = Hunting(g);
            var husk = sim.Swarm.Spawn(AlienKind.Husk, g.IdOf("nest"), 60);
            var perRoom = SwarmRules.Default.MoveTicks(AlienKind.Husk);

            RunTicks(sim, perRoom);
            Assert.That(sim.Swarm.RoomOf(husk), Is.EqualTo(g.IdOf("b")));
            RunTicks(sim, 2 * perRoom);
            Assert.That(sim.Swarm.RoomOf(husk), Is.EqualTo(g.IdOf("bay")));
        }

        [Test]
        public void Hunting_ACrawlerTakesTheDuct()
        {
            var g = Ship();
            var sim = Hunting(g);
            var crawler = sim.Swarm.Spawn(AlienKind.Crawler, g.IdOf("nest"), 40);
            var perRoom = SwarmRules.Default.MoveTicks(AlienKind.Crawler);

            RunTicks(sim, perRoom);
            Assert.That(sim.Swarm.RoomOf(crawler), Is.EqualTo(g.IdOf("a")), "through the duct, skipping b");
            RunTicks(sim, perRoom);
            Assert.That(sim.Swarm.RoomOf(crawler), Is.EqualTo(g.IdOf("bay")));
        }

        [Test]
        public void AClosedDoor_CostsForcingTime()
        {
            var g = Ship();
            var sim = Hunting(g);
            sim.SetDoor(g.IdOf("b"), g.IdOf("nest"), DoorState.Closed);
            var husk = sim.Swarm.Spawn(AlienKind.Husk, g.IdOf("nest"), 60);
            var rules = SwarmRules.Default;

            RunTicks(sim, rules.MoveTicks(AlienKind.Husk));
            Assert.That(sim.Swarm.RoomOf(husk), Is.EqualTo(g.IdOf("nest")), "still forcing the door");
            RunTicks(sim, rules.ForceDoorTicks(AlienKind.Husk));
            Assert.That(sim.Swarm.RoomOf(husk), Is.EqualTo(g.IdOf("b")));
        }

        [Test]
        public void Restless_AliensInvestigateTheNoise()
        {
            var g = Ship();
            var sim = new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });
            sim.SetShuttle(g.IdOf("bay"));
            sim.ForceStage(HiveStage.Restless);
            sim.AddNoiseSource(g.IdOf("b"), 6); // a Joe cutting in b
            var husk = sim.Swarm.Spawn(AlienKind.Husk, g.IdOf("nest"), 60);

            RunTicks(sim, SwarmRules.Default.MoveTicks(AlienKind.Husk) * 3);
            Assert.That(sim.Swarm.RoomOf(husk), Is.EqualTo(g.IdOf("b")), "it went to the sound and stayed");
        }

        [Test]
        public void Moves_AreEvents()
        {
            var g = Ship();
            var sim = Hunting(g);
            var seen = new List<SimEvent>();
            sim.Subscribe(new Collector(seen));
            var husk = sim.Swarm.Spawn(AlienKind.Husk, g.IdOf("nest"), 60);

            RunTicks(sim, SwarmRules.Default.MoveTicks(AlienKind.Husk));
            var move = seen.Single(e => e.Kind == SimEventKind.AlienEnteredRoom);
            Assert.That(move.Alien, Is.EqualTo(husk));
            Assert.That(move.Room, Is.EqualTo(g.IdOf("b")));
            Assert.That(sim.Events.Any(e => e.Kind == SimEventKind.AlienEnteredRoom), Is.False, "moves are transient, not journalled");
        }

        private sealed class Collector : ISimEventHandler
        {
            private readonly List<SimEvent> _into;
            public Collector(List<SimEvent> into) => _into = into;
            public void Handle(in SimEvent e) => _into.Add(e);
        }
    }
}
