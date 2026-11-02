using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;
using UnityEngine;

namespace QuietHull.Data.Tests
{
    /// <summary>The commander's interface drifting from company-clean to biomechanical with the infection.</summary>
    public sealed class UiThemeTests
    {
        private InMemoryDefinitionFactory _factory;
        private UiThemeContent _ui;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _ui = UiThemeBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void EveryBlend_KeepsWcagContrast()
        {
            var errors = _ui.Catalog.Validate().Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void ContrastMaths_MatchesWcag()
        {
            Assert.That(ColorMath.Contrast(ColorMath.FromHex("#000000"), ColorMath.FromHex("#FFFFFF")), Is.EqualTo(21f).Within(0.01f));
            Assert.That(ColorMath.Contrast(ColorMath.FromHex("#777777"), ColorMath.FromHex("#FFFFFF")), Is.EqualTo(4.48f).Within(0.02f));
        }

        [Test]
        public void Blend_HitsItsEndpoints()
        {
            var a = ColorMath.FromHex("#F2A541");
            var b = ColorMath.FromHex("#9FB3B8");
            var start = ColorMath.Blend(a, b, 0f);
            var end = ColorMath.Blend(a, b, 1f);
            Assert.That(start.r, Is.EqualTo(a.r).Within(0.002f));
            Assert.That(start.g, Is.EqualTo(a.g).Within(0.002f));
            Assert.That(end.b, Is.EqualTo(b.b).Within(0.002f));
        }

        [Test]
        public void StageOne_IsCompanyIndustrial_StageFive_IsBiomechanical()
        {
            var clean = _ui.CommanderTheme.Evaluate(0f);
            var alien = _ui.CommanderTheme.Evaluate(1f);
            Assert.That(clean.organicFrames, Is.Zero);
            Assert.That(clean.pulse, Is.Zero);
            Assert.That(alien.organicFrames, Is.EqualTo(1f));
            Assert.That(alien.pulse, Is.GreaterThan(0f));
            Assert.That(ColorMath.Luminance(alien.background), Is.LessThan(ColorMath.Luminance(clean.background)), "it darkens");
        }

        [Test]
        public void TheEffects_OnlyGrowAsMarlowChanges()
        {
            var samples = Enumerable.Range(0, 21).Select(i => _ui.CommanderTheme.Evaluate(i / 20f)).ToList();
            for (var i = 1; i < samples.Count; i++)
            {
                Assert.That(samples[i].organicFrames, Is.GreaterThanOrEqualTo(samples[i - 1].organicFrames));
                Assert.That(samples[i].pulse, Is.GreaterThanOrEqualTo(samples[i - 1].pulse));
                Assert.That(samples[i].sheen, Is.GreaterThanOrEqualTo(samples[i - 1].sheen));
            }
        }

        [Test]
        public void Drift_FollowsTheInfection_SplicesAndLevers()
        {
            Assert.That(InfectionUiDrift.Drift(1, 0, 0, UiDriftSetting.Full), Is.Zero, "Stage I looks human");
            var previous = -1f;
            for (var stage = 1; stage <= 5; stage++)
            {
                var d = InfectionUiDrift.Drift(stage, 0, 0, UiDriftSetting.Full);
                Assert.That(d, Is.GreaterThan(previous));
                previous = d;
            }

            Assert.That(InfectionUiDrift.Drift(3, 2, 3, UiDriftSetting.Full), Is.GreaterThan(InfectionUiDrift.Drift(3, 0, 0, UiDriftSetting.Full)),
                "splices and levers push it further");
            Assert.That(InfectionUiDrift.Drift(5, 6, 6, UiDriftSetting.Full), Is.EqualTo(1f));
        }

        [Test]
        public void Accessibility_ReducedHalves_OffKeepsTheHumanLook_HudStaysReadable()
        {
            var full = InfectionUiDrift.Drift(4, 1, 1, UiDriftSetting.Full);
            Assert.That(InfectionUiDrift.Drift(4, 1, 1, UiDriftSetting.Reduced), Is.EqualTo(full * 0.5f).Within(1e-6f));
            Assert.That(InfectionUiDrift.Drift(5, 3, 3, UiDriftSetting.Off), Is.Zero);
            Assert.That(InfectionUiDrift.HudDrift(1f), Is.EqualTo(InfectionUiDrift.HudCap));
            Assert.Throws<ArgumentOutOfRangeException>(() => InfectionUiDrift.Drift(6, 0, 0, UiDriftSetting.Full));
        }
    }
}
