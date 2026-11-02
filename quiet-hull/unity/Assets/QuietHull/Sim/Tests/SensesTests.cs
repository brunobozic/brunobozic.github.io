using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>Sound, vibration and heat travel differently; aliens sense them differently.</summary>
    public sealed class SensesTests
    {
        // bay - corridor - crusher - nest. The crusher's machinery is loud and shakes the deck.
        private static HulkGraph Ship() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay")
                .Room("corridor", "Corridor")
                .Room("crusher", "Crusher bay", ambientNoise: 6, ambientVibration: 5, ambientHeat: 2)
                .Room("nest", "Nest")
                .Door("bay", "corridor")
                .Door("corridor", "crusher")
                .Door("crusher", "nest")
                .Build();

        private static void RunSeconds(Simulation sim, int s)
        {
            for (var i = 0; i < s * SimTime.TicksPerSecond; i++) sim.Step();
        }

        [Test]
        public void Vibration_TravelsThroughTheHull_PastSealedDoors()
        {
            var g = Ship();
            var doors = new DoorStates(g);
            doors.Set(g.IdOf("bay"), g.IdOf("corridor"), DoorState.Sealed);

            var raw = SenseField.Spread(g, doors, SenseRules.Default, SenseChannel.Vibration, g.IdOf("bay"), 6);
            Assert.That(raw[g.IdOf("corridor").Index], Is.EqualTo(5), "1 lost per hop; doors don't matter");
            Assert.That(raw[g.IdOf("nest").Index], Is.EqualTo(3));
        }

        [Test]
        public void Heat_StaysLocal()
        {
            var g = Ship();
            var raw = SenseField.Spread(g, new DoorStates(g), SenseRules.Default, SenseChannel.Heat, g.IdOf("bay"), 6);
            Assert.That(raw[g.IdOf("corridor").Index], Is.EqualTo(3), "3 lost per hop");
            Assert.That(raw[g.IdOf("crusher").Index], Is.Zero);
        }

        [Test]
        public void Heat_SoaksThroughASealedDoor_AsIfClosed()
        {
            var g = Ship();
            var doors = new DoorStates(g);
            doors.Set(g.IdOf("bay"), g.IdOf("corridor"), DoorState.Sealed);
            var raw = SenseField.Spread(g, doors, SenseRules.Default, SenseChannel.Heat, g.IdOf("bay"), 8);
            Assert.That(raw[g.IdOf("corridor").Index], Is.EqualTo(3), "8 - 3 per hop - 2 for the door");
        }

        [Test]
        public void ASilentButHeavyLoad_IsFeltByAVibrationHunter_NotByAnEar()
        {
            var g = Ship();
            // A loaded hauler: quiet (sound 2) but heavy (vibration 7), in the corridor.
            var ears = new Simulation(g, SimRules.Default, new[] { new HiveListener(g.IdOf("nest"), SenseWeights.EarsOnly) });
            var feelers = new Simulation(g, SimRules.Default, new[] { new HiveListener(g.IdOf("nest"), SenseWeights.VibrationOnly) });
            foreach (var sim in new[] { ears, feelers })
            {
                sim.AddEmitter(g.IdOf("corridor"), new Emission(sound: 2, vibration: 7, heat: 0));
            }

            RunSeconds(ears, 5);
            RunSeconds(feelers, 5);
            Assert.That(ears.Awareness.Value, Is.Zero, "the sound dies before the nest");
            Assert.That(feelers.Awareness.Value, Is.EqualTo(25f).Within(1e-3f), "7 - 2 hops = 5 felt, for 5 s");
        }

        [Test]
        public void TheCrushersShaking_MasksVibration_InsideIt()
        {
            var g = new HulkGraphBuilder()
                .Room("crusher", "Crusher bay", ambientVibration: 5)
                .Room("nest", "Nest", ambientVibration: 5)
                .Door("crusher", "nest")
                .Build();
            var sim = new Simulation(g, SimRules.Default, new[] { new HiveListener(g.IdOf("nest"), SenseWeights.VibrationOnly) });
            sim.AddEmitter(g.IdOf("crusher"), new Emission(0, 6, 0)); // 5 arrives; the nest shakes at 5 anyway
            RunSeconds(sim, 5);
            Assert.That(sim.Awareness.Value, Is.Zero);
        }

        [Test]
        public void Weights_ScaleWhatEachSenseContributes()
        {
            var g = Ship();
            var half = new Simulation(g, SimRules.Default, new[] { new HiveListener(g.IdOf("nest"), new SenseWeights(50, 0, 0)) });
            half.AddNoiseSource(g.IdOf("crusher"), 9); // 7 reaches the nest
            RunSeconds(half, 10);
            Assert.That(half.Awareness.Value, Is.EqualTo(30f).Within(1e-3f), "7 at 50% rounds down to 3 per second");
        }
    }
}
