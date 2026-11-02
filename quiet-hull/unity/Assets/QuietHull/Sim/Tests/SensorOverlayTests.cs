using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>Sentry pups as sensors: the map overlay only shows what a pup can read.</summary>
    public sealed class SensorOverlayTests
    {
        // bay - corridor - crusher - gallery - nest
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay")
                .Room("corridor", "Corridor")
                .Room("crusher", "Crusher bay", ambientNoise: 6, ambientVibration: 5, ambientHeat: 2)
                .Room("gallery", "Valve gallery")
                .Room("nest", "Nest")
                .Door("bay", "corridor")
                .Door("corridor", "crusher")
                .Door("crusher", "gallery")
                .Door("gallery", "nest")
                .Build();

        private static Simulation NewSim(HulkGraph g) => new Simulation(g, SimRules.Default, new[] { g.IdOf("nest") });

        [Test]
        public void WithoutPups_TheMapShowsNothing()
        {
            var g = Ship();
            var sim = NewSim(g);
            var overlay = new SensorOverlay(g);
            sim.ReadOverlay(overlay);
            for (var r = 0; r < g.RoomCount; r++)
            {
                Assert.That(overlay.At(new RoomId(r)).Covered, Is.False);
            }
        }

        [Test]
        public void APup_CoversItsRoom_AndRoomsWithinRange()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.PlaceSensor(g.IdOf("corridor"), rangeHops: 1);
            var overlay = new SensorOverlay(g);
            sim.ReadOverlay(overlay);

            Assert.That(overlay.At(g.IdOf("corridor")).Covered, Is.True);
            Assert.That(overlay.At(g.IdOf("bay")).Covered, Is.True);
            Assert.That(overlay.At(g.IdOf("crusher")).Covered, Is.True);
            Assert.That(overlay.At(g.IdOf("gallery")).Covered, Is.False);
        }

        [Test]
        public void TheOverlay_ShowsAllThreeSenses_AndWhenWorkIsMasked()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.PlaceSensor(g.IdOf("crusher"), rangeHops: 1);
            sim.AddEmitter(g.IdOf("crusher"), new Emission(sound: 6, vibration: 4, heat: 5)); // a cart and a hot Joe

            var overlay = new SensorOverlay(g);
            sim.ReadOverlay(overlay);
            var crusher = overlay.At(g.IdOf("crusher"));
            Assert.That(crusher.Level(SenseChannel.Sound), Is.EqualTo(6));
            Assert.That(crusher.Masked(SenseChannel.Sound), Is.True, "6 in a room that already roars at 6");
            Assert.That(crusher.Masked(SenseChannel.Vibration), Is.True, "4 under the deck's 5");
            Assert.That(crusher.Level(SenseChannel.Heat), Is.EqualTo(5));
            Assert.That(crusher.Masked(SenseChannel.Heat), Is.False, "5 over the room's 2: heat-hunters will see it");

            var gallery = overlay.At(g.IdOf("gallery"));
            Assert.That(gallery.Level(SenseChannel.Sound), Is.EqualTo(4));
            Assert.That(gallery.Masked(SenseChannel.Sound), Is.False, "the quiet gallery carries it on to the nest");
        }

        [Test]
        public void RemovingThePup_DarkensItsRooms()
        {
            var g = Ship();
            var sim = NewSim(g);
            var pup = sim.PlaceSensor(g.IdOf("corridor"), 1);
            sim.RemoveSensor(pup);
            var overlay = new SensorOverlay(g);
            sim.ReadOverlay(overlay);
            Assert.That(overlay.At(g.IdOf("corridor")).Covered, Is.False);
        }

#if NETCOREAPP
        [Test]
        public void ReadingTheOverlay_EveryFrame_AllocatesNothing()
        {
            var g = Ship();
            var sim = NewSim(g);
            sim.PlaceSensor(g.IdOf("corridor"), 2);
            sim.AddEmitter(g.IdOf("crusher"), new Emission(6, 4, 5));
            var overlay = new SensorOverlay(g);
            sim.ReadOverlay(overlay);

            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (var frame = 0; frame < 600; frame++)
            {
                sim.Step();
                sim.ReadOverlay(overlay);
            }

            Assert.That(System.GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
        }
#endif
    }
}
