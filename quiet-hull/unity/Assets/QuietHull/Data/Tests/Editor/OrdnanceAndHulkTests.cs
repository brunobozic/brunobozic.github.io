using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>Mines, contraband scans, chassis marks, sun-locked rooms and belt erosion.</summary>
    public sealed class OrdnanceAndHulkTests
    {
        private InMemoryDefinitionFactory _factory;
        private ColdSleepContent _c;
        private OrdnanceCatalog _o;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _c = ColdSleepContentBuilder.Build(_factory);
            _o = _c.Ordnance;
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void Catalogs_HaveNoValidationErrors()
        {
            var (sun, _) = SunwheelSampleBuilder.Build(_factory);
            var reports = new[] { _o.Catalog.Validate(), _factory.GetOrCreateCatalog(SunwheelSampleBuilder.CatalogId).Validate(), _c.Catalog.Validate() };
            var errors = reports.SelectMany(r => r.Issues).Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();

            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
            Assert.That(sun, Is.Not.Null);
        }

        // ---- Mines -----------------------------------------------------------------------------

        [Test]
        public void Frag_KillsCrawlersAndSpitters_ButNotLeapers_ShredderKillsLeapers()
        {
            Assert.That(_o.Frag.Kills(_c.Prologue.Crawler), Is.True);
            Assert.That(_o.Frag.Kills(_c.Prologue.Spitter), Is.True);
            Assert.That(_o.Frag.Kills(_c.Leaper), Is.False);
            Assert.That(_o.Shredder.Kills(_c.Leaper), Is.True);
        }

        [Test]
        public void Stun_OutlastsALeapersCrouch()
        {
            Assert.That(_o.Stun.StunSeconds, Is.GreaterThan(1f));
            Assert.That(_o.Stun.Damage, Is.Zero);
        }

        [Test]
        public void Bloodhound_IgnoresYourUnits_FragDoesNot()
        {
            Assert.That(_o.Bloodhound.TriggersOnPlayerUnits, Is.False);
            Assert.That(_o.Frag.TriggersOnPlayerUnits, Is.True);
        }

        [Test]
        public void Lure_OutshoutsTheCrusherBay_AndMinesAreAlarms()
        {
            Assert.That(_o.Lure.IsLure, Is.True);
            Assert.That(_o.Lure.LureNoise, Is.GreaterThan(_c.Prologue.CrusherBay.PeakLevel));
            Assert.That(_o.Mines.Where(m => m.Damage > 0f).All(m => m.DetonationNoise >= 10f || m == _o.Cryo), Is.True,
                "blast mines are loud; only the cryo mine is quiet");
        }

        [Test]
        public void Sapper_PlantsFaster()
        {
            var plain = _o.Frag.PlantSecondsWith(Array.Empty<StatModifier>());
            var sapper = _o.Frag.PlantSecondsWith(_c.Augs.Sapper.Passives);

            Assert.That(sapper, Is.LessThan(plain * 0.7f));
        }

        [Test]
        public void BasicMinesAreForSale_TheRestAreTeasers()
        {
            Assert.That(_c.MineOffers, Has.Count.EqualTo(3));
            Assert.That(_c.MineOffers.All(o => o.CanBuyNow), Is.True);
            Assert.That(_c.TeaserOffers.Count(o => o.Target is BlueprintDefinition bp && bp.Unlocks.Output is MineDefinition), Is.EqualTo(3));
        }

        // ---- Contraband ----------------------------------------------------------------------

        [Test]
        public void Compartment_HidesContrabandNineTimesInTen()
        {
            var compartment = _o.HiddenCompartment;

            Assert.That(ContrabandMath.DetectionChance(false, compartment), Is.EqualTo(0.6f).Within(1e-5f));
            Assert.That(ContrabandMath.DetectionChance(true, compartment), Is.EqualTo(0.1f).Within(1e-5f));
            Assert.That(ContrabandMath.ChanceAnythingFound(0, 8, compartment), Is.EqualTo(0.1f).Within(1e-5f), "the compartment is one check, not eight");
            Assert.That(ContrabandMath.ChanceAnythingFound(1, 0, compartment), Is.EqualTo(0.6f).Within(1e-5f));
            Assert.That(compartment.HiddenUnitBerths, Is.EqualTo(1), "room for one shadow-fitted synth");
            Assert.That(_c.TeaserOffers.Any(o => o.Target == compartment), Is.True);
        }

        // ---- Chassis marks ---------------------------------------------------------------------

        [Test]
        public void MilitaryChassis_GoesToMkVIII()
        {
            var mk8 = _c.Picket.StatsAt(Mark.MkVIII);

            Assert.That(_c.Picket.MaxMark, Is.EqualTo(Mark.MkVIII));
            Assert.That(mk8.MaxHealth, Is.EqualTo(_c.Picket.MaxHealth * 1.84f).Within(0.5f));
            Assert.That(mk8.PowerOutputWatts, Is.EqualTo(_c.Picket.BasePowerOutputWatts + 280f).Within(0.01f));
        }

        // ---- Sun-locked rooms and the belt ------------------------------------------------------

        [Test]
        public void EveryTurn_OpensSomeRooms_AndClosesOthers()
        {
            var (sun, _) = SunwheelSampleBuilder.Build(_factory);

            for (var facing = 0; facing < sun.Facings; facing++)
            {
                var (unlocked, locked) = sun.Turn(facing, facing + 1);
                Assert.That(unlocked, Is.Not.Empty, $"turn {facing}→{facing + 1} opens nothing");
                Assert.That(locked, Is.Not.Empty, $"turn {facing}→{facing + 1} closes nothing");
            }
        }

        [Test]
        public void TheGoalRooms_AreNeverAllOpenAtOnce_SoItTakesSeveralTurns()
        {
            var (sun, _) = SunwheelSampleBuilder.Build(_factory);
            var goals = new[] { "fabricator_t2", "cryo_vault", "armoury" };

            for (var facing = 0; facing < sun.Facings; facing++)
            {
                Assert.That(goals.All(sun.OpenRooms(facing).Contains), Is.False, $"facing {facing} opens every goal");
            }

            foreach (var goal in goals)
            {
                Assert.That(Enumerable.Range(0, sun.Facings).Any(f => !sun.IsSunLocked(goal, f)), Is.True, goal);
            }
        }

        [Test]
        public void TurningTheNestIntoTheSun_BurnsCrawlersFast_SpittersSlowly()
        {
            var (sun, _) = SunwheelSampleBuilder.Build(_factory);
            var heat = sun.HeatOf("nest", 2);

            Assert.That(heat, Is.EqualTo(3));
            Assert.That(_c.Prologue.Crawler.SecondsToBurn(heat), Is.LessThan(2f));
            Assert.That(_c.Prologue.Spitter.SecondsToBurn(heat), Is.InRange(4f, 10f));
            Assert.That(_c.Prologue.Spitter.SecondsToBurn(2), Is.EqualTo(float.PositiveInfinity), "Spitters stand Heat 2");
        }

        [Test]
        public void TheBelt_VentsTheNestOrStripsTheReactor_DependingOnTheFacingYouHold()
        {
            var (_, belt) = SunwheelSampleBuilder.Build(_factory);

            Assert.That(belt.Find("nest_hull").outcome, Is.EqualTo(ErosionOutcome.Breach));
            Assert.That(belt.SecondsToFail("nest_hull", 2), Is.EqualTo(300f).Within(0.01f), "hold facing 2 for five minutes to vent the nest");
            Assert.That(belt.SecondsToFail("nest_hull", 3), Is.EqualTo(float.PositiveInfinity));

            var reactor = belt.Find("reactor_housing");
            Assert.That(reactor.outcome, Is.EqualTo(ErosionOutcome.StripRadiationSource));
            Assert.That(reactor.outcomeTargetRoom, Is.EqualTo("deep_hold"));
            Assert.That(belt.SecondsToFail("reactor_housing", 3), Is.EqualTo(600f).Within(0.01f));
            Assert.That(belt.SecondsToFail("spine_truss", 0), Is.LessThan(float.PositiveInfinity), "waiting at the start facing has a cost too");
        }
    }
}
