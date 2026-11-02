using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>Shared route maps: one Dijkstra per goal, read by every alien.</summary>
    public sealed class FlowFieldTests
    {
        // bay - a - b - nest, plus a long way round bay - c - d - e - nest, and a duct a ~ nest.
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Bay").Room("a", "A").Room("b", "B").Room("nest", "Nest")
                .Room("c", "C").Room("d", "D").Room("e", "E")
                .Door("bay", "a").Door("a", "b").Door("b", "nest")
                .Door("bay", "c").Door("c", "d").Door("d", "e").Door("e", "nest")
                .Duct("a", "nest")
                .Build();

        [Test]
        public void Distance_CountsHops_FromTheGoal()
        {
            var g = Ship();
            var field = new FlowField(g, SwarmRules.Default, useDucts: false);
            field.Compute(new DoorStates(g), new[] { g.IdOf("bay") });

            var hop = SwarmRules.Default.HopCost;
            Assert.That(field.DistanceOf(g.IdOf("bay")), Is.Zero);
            Assert.That(field.DistanceOf(g.IdOf("a")), Is.EqualTo(hop));
            Assert.That(field.DistanceOf(g.IdOf("nest")), Is.EqualTo(3 * hop));
        }

        [Test]
        public void ADuctIsAShortcut_OnlyForThoseWhoUseDucts()
        {
            var g = Ship();
            var walkers = new FlowField(g, SwarmRules.Default, useDucts: false);
            var crawlers = new FlowField(g, SwarmRules.Default, useDucts: true);
            walkers.Compute(new DoorStates(g), new[] { g.IdOf("bay") });
            crawlers.Compute(new DoorStates(g), new[] { g.IdOf("bay") });

            var hop = SwarmRules.Default.HopCost;
            Assert.That(walkers.DistanceOf(g.IdOf("nest")), Is.EqualTo(3 * hop));
            Assert.That(crawlers.DistanceOf(g.IdOf("nest")), Is.EqualTo(2 * hop), "nest ~ a - bay");
        }

        [Test]
        public void SealedDoors_SendTheSwarmTheLongWay()
        {
            var g = Ship();
            var doors = new DoorStates(g);
            doors.Set(g.IdOf("bay"), g.IdOf("a"), DoorState.Sealed);
            var field = new FlowField(g, SwarmRules.Default, useDucts: false);
            field.Compute(doors, new[] { g.IdOf("bay") });

            Assert.That(field.DistanceOf(g.IdOf("nest")), Is.EqualTo(4 * SwarmRules.Default.HopCost), "round by c, d, e");
            Assert.That(field.NextStep(g.IdOf("nest"), doors).Room, Is.EqualTo(g.IdOf("e")));
        }

        [Test]
        public void IfEveryWayIsSealed_TheyBreakThrough()
        {
            var g = new HulkGraphBuilder().Room("bay", "Bay").Room("nest", "Nest").Door("bay", "nest").Build();
            var doors = new DoorStates(g);
            doors.Set(g.IdOf("bay"), g.IdOf("nest"), DoorState.Sealed);
            var field = new FlowField(g, SwarmRules.Default, useDucts: false);
            field.Compute(doors, new[] { g.IdOf("bay") });

            Assert.That(field.DistanceOf(g.IdOf("nest")), Is.EqualTo(SwarmRules.Default.HopCost + SwarmRules.Default.SealedPenalty));
        }
    }
}
