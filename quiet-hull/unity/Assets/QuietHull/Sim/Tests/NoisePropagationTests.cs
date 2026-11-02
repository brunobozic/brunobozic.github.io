using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    public sealed class NoisePropagationTests
    {
        // bay - corridor - crusher - gallery, with a side vent room off the corridor.
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay")
                .Room("corridor", "Corridor")
                .Room("crusher", "Crusher bay", ambientNoise: 6)
                .Room("gallery", "Valve gallery")
                .Room("vent", "Vent room")
                .Door("bay", "corridor")
                .Door("corridor", "crusher")
                .Door("crusher", "gallery")
                .Door("corridor", "vent")
                .Build();

        private static readonly NoiseRules Rules = NoiseRules.Default; // 2 lost per hop, a closed door costs 3 more

        [Test]
        public void Noise_FallsOffByHop()
        {
            var g = Ship();
            var doors = new DoorStates(g);
            var field = NoisePropagation.Spread(g, doors, Rules, g.IdOf("bay"), loudness: 8);

            Assert.That(field.RawAt(g.IdOf("bay")), Is.EqualTo(8));
            Assert.That(field.RawAt(g.IdOf("corridor")), Is.EqualTo(6));
            Assert.That(field.RawAt(g.IdOf("crusher")), Is.EqualTo(4));
            Assert.That(field.RawAt(g.IdOf("gallery")), Is.EqualTo(2));
            Assert.That(field.RawAt(g.IdOf("vent")), Is.EqualTo(4));
        }

        [Test]
        public void Ambient_MasksWhatTheHiveHears()
        {
            var g = Ship();
            var field = NoisePropagation.Spread(g, new DoorStates(g), Rules, g.IdOf("bay"), loudness: 8);

            // The crusher's own racket (6) drowns the 4 that arrives there.
            Assert.That(field.HeardAt(g.IdOf("crusher")), Is.EqualTo(0));
            Assert.That(field.HeardAt(g.IdOf("corridor")), Is.EqualTo(6));
        }

        [Test]
        public void ANoisyJob_InALoudRoom_IsMasked()
        {
            var g = Ship();
            // A cart (6) in the crusher bay (ambient 6) is silent there, and only its falloff is heard elsewhere.
            var field = NoisePropagation.Spread(g, new DoorStates(g), Rules, g.IdOf("crusher"), loudness: 6);
            Assert.That(field.HeardAt(g.IdOf("crusher")), Is.EqualTo(0));
            Assert.That(field.HeardAt(g.IdOf("gallery")), Is.EqualTo(4));
        }

        [Test]
        public void ClosedDoors_Muffle_SealedDoors_Block()
        {
            var g = Ship();
            var doors = new DoorStates(g);
            doors.Set(g.IdOf("corridor"), g.IdOf("crusher"), DoorState.Closed);
            doors.Set(g.IdOf("corridor"), g.IdOf("vent"), DoorState.Sealed);

            var field = NoisePropagation.Spread(g, doors, Rules, g.IdOf("bay"), loudness: 8);
            Assert.That(field.RawAt(g.IdOf("crusher")), Is.EqualTo(1), "6 at the corridor, minus 2 for the hop and 3 for the closed door");
            Assert.That(field.RawAt(g.IdOf("vent")), Is.EqualTo(0), "a sealed door stops sound");
        }

        [Test]
        public void Noise_NeverGoesNegative_AndTakesTheLoudestPath()
        {
            var g = new HulkGraphBuilder()
                .Room("a", "A").Room("b", "B").Room("c", "C").Room("d", "D")
                .Door("a", "b").Door("b", "d").Door("a", "c").Door("c", "d")
                .Build();
            var doors = new DoorStates(g);
            doors.Set(g.IdOf("a"), g.IdOf("b"), DoorState.Closed);

            var field = NoisePropagation.Spread(g, doors, Rules, g.IdOf("a"), loudness: 5);
            Assert.That(field.RawAt(g.IdOf("d")), Is.EqualTo(1), "via the open side: 5 - 2 - 2");

            var quiet = NoisePropagation.Spread(g, doors, Rules, g.IdOf("a"), loudness: 1);
            Assert.That(quiet.RawAt(g.IdOf("d")), Is.EqualTo(0));
        }
    }
}
