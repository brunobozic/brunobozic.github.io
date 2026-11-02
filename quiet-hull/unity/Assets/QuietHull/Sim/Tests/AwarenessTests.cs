using System.Collections.Generic;
using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    public sealed class AwarenessTests
    {
        private static readonly AwarenessRules Rules = AwarenessRules.Default; // gain 1 per heard point per second, decay 2/s, Restless 40, Hunt 80

        [Test]
        public void TheHive_StartsDormant()
        {
            var gauge = new AwarenessGauge(Rules);
            Assert.That(gauge.Value, Is.Zero);
            Assert.That(gauge.Stage, Is.EqualTo(HiveStage.Dormant));
        }

        [Test]
        public void HeardNoise_FillsTheGauge_OverTime()
        {
            var gauge = new AwarenessGauge(Rules);
            for (var tick = 0; tick < SimTime.TicksPerSecond * 5; tick++)
            {
                gauge.Tick(heardNoise: 4);
            }

            Assert.That(gauge.Value, Is.EqualTo(20f).Within(1e-3f), "4 heard for 5 s at 1 per point per second");
        }

        [Test]
        public void Silence_DrainsIt()
        {
            var gauge = new AwarenessGauge(Rules);
            for (var t = 0; t < SimTime.TicksPerSecond * 5; t++) gauge.Tick(4);
            for (var t = 0; t < SimTime.TicksPerSecond * 3; t++) gauge.Tick(0);
            Assert.That(gauge.Value, Is.EqualTo(14f).Within(1e-3f));

            for (var t = 0; t < SimTime.TicksPerSecond * 30; t++) gauge.Tick(0);
            Assert.That(gauge.Value, Is.Zero, "never below zero");
        }

        [Test]
        public void Stages_RiseAtTheThresholds_AndNeverGoBack()
        {
            var gauge = new AwarenessGauge(Rules);
            var changes = new List<HiveStage>();
            gauge.StageChanged += (from, to) => changes.Add(to);

            for (var t = 0; t < SimTime.TicksPerSecond * 10; t++) gauge.Tick(4);   // 40
            Assert.That(gauge.Stage, Is.EqualTo(HiveStage.Restless));

            for (var t = 0; t < SimTime.TicksPerSecond * 20; t++) gauge.Tick(0);   // drains to 0
            Assert.That(gauge.Stage, Is.EqualTo(HiveStage.Restless), "stages never go back");

            for (var t = 0; t < SimTime.TicksPerSecond * 10; t++) gauge.Tick(8);   // 80
            Assert.That(gauge.Stage, Is.EqualTo(HiveStage.Hunt));
            Assert.That(changes, Is.EqualTo(new[] { HiveStage.Restless, HiveStage.Hunt }), "one event per stage");
        }

        [Test]
        public void TheGauge_IsCapped()
        {
            var gauge = new AwarenessGauge(Rules);
            for (var t = 0; t < SimTime.TicksPerSecond * 60; t++) gauge.Tick(10);
            Assert.That(gauge.Value, Is.EqualTo(Rules.Maximum));
        }
    }
}
