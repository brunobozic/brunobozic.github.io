using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>Automatic salvage, worker autonomy, swarm rerouting and broker loans.</summary>
    public sealed class SalvageAutonomyLoanTests
    {
        private InMemoryDefinitionFactory _factory;
        private TremolinoContent _t;
        private ColdSleepContent _c;
        private AugmentCatalog _a;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _t = TremolinoContentBuilder.Build(_factory);
            _c = _t.Previous;
            _a = _c.Augs;
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        private LoadoutProfile Worker(Mark mark, params InstalledAugment[] modules) => LoadoutProfile.Compute(_c.SecondWorker, mark, modules);

        private List<LoadoutProfile> Crew(int count, Func<LoadoutProfile> make) => Enumerable.Range(0, count).Select(_ => make()).ToList();

        // ---- Automatic salvage ----------------------------------------------------------------

        [Test]
        public void EveryMission_PaysSomeSalvage_EvenWithNoWorkers()
        {
            Assert.That(new[] { _c.Trip1, _c.Judea, _c.Trip2, _t.Mission }.All(m => m.BaseAutoSalvageScrip > 0), Is.True);
            Assert.That(WorkerMath.AutoSalvage(_c.Trip1.BaseAutoSalvageScrip, Array.Empty<LoadoutProfile>(), 4, 0f), Is.EqualTo(250));
        }

        [Test]
        public void MoreWorkers_AndSalvageUpgrades_PayMore_AndAFullFittedCrewIsATangibleBonus()
        {
            var baseScrip = _t.Mission.BaseAutoSalvageScrip;
            var plain = Crew(2, () => Worker(Mark.MkI));
            var fourPlain = Crew(4, () => Worker(Mark.MkI));
            var fourFitted = Crew(4, () => Worker(Mark.MkI, new InstalledAugment(_a.SalvageScanner, Mark.MkI)));

            var two = WorkerMath.AutoSalvage(baseScrip, plain, 4, 0f);
            var four = WorkerMath.AutoSalvage(baseScrip, fourPlain, 4, 0f);
            var full = WorkerMath.AutoSalvage(baseScrip, fourFitted, 4, 0f);

            Assert.That(two, Is.GreaterThan(baseScrip));
            Assert.That(four, Is.GreaterThan(two));
            Assert.That(full, Is.GreaterThanOrEqualTo(2.4f * baseScrip), $"a full fitted crew: {full} vs base {baseScrip}");
            Assert.That(full / (float)four, Is.GreaterThan(1.5f), "the full-crew bonus is felt");
        }

        [Test]
        public void Cards_ModifySalvage()
        {
            var crew = Crew(2, () => Worker(Mark.MkI));
            var plain = WorkerMath.AutoSalvage(400, crew, 4, 0f);
            var gleaner = WorkerMath.AutoSalvage(400, crew, 4, _c.GleanersMoon.ValueOf(OmenEffectKind.SalvagePercent));

            Assert.That(gleaner, Is.EqualTo((int)Math.Round(plain * 1.4f)).Within(1));
        }

        // ---- Autonomy -------------------------------------------------------------------------

        [Test]
        public void AStockWorker_CantScavenge_AQuietSmartOneCan()
        {
            var stock = Worker(Mark.MkI);
            var quietSmart = Worker(Mark.MkI,
                new InstalledAugment(_a.SurveyAi, Mark.MkI),
                new InstalledAugment(_a.MuffledFrame, Mark.MkI));
            var loudSmart = Worker(Mark.MkI,
                new InstalledAugment(_a.SurveyAi, Mark.MkI),
                new InstalledAugment(_a.LoaderFrame, Mark.MkI));

            Assert.That(WorkerMath.CanScavenge(_c.SecondWorker, Mark.MkI, stock), Is.False);
            Assert.That(WorkerMath.CanScavenge(_c.SecondWorker, Mark.MkI, quietSmart), Is.True);
            Assert.That(WorkerMath.CanScavenge(_c.SecondWorker, Mark.MkI, loudSmart), Is.False, "smart isn't enough: it must be quiet");
        }

        [Test]
        public void SentryTactics_NeedAutonomy3_FromBetterModulesOrNewerFirmware()
        {
            var survey1 = Worker(Mark.MkI, new InstalledAugment(_a.SurveyAi, Mark.MkI));
            var survey4 = Worker(Mark.MkI, new InstalledAugment(_a.SurveyAi, Mark.MkIV));
            var gen2 = Worker(Mark.MkV, new InstalledAugment(_a.SurveyAi, Mark.MkI));

            Assert.That(WorkerMath.CanRunSentryTactics(_c.SecondWorker, Mark.MkI, survey1), Is.False);
            Assert.That(WorkerMath.CanRunSentryTactics(_c.SecondWorker, Mark.MkI, survey4), Is.True, "a Mk IV Survey AI");
            Assert.That(WorkerMath.CanRunSentryTactics(_c.SecondWorker, Mark.MkV, gen2), Is.True, "a Gen 2 worker with a Mk I Survey AI");
            Assert.That(_c.SecondWorker.MarkCurve.FirmwareAutonomyAt(Mark.MkIX), Is.EqualTo(2));
        }

        // ---- Swarm rerouting --------------------------------------------------------------------

        [Test]
        public void TurningTheHulk_ReroutesTheSwarm()
        {
            var sun = _t.Sun;
            var tolerance = _c.Prologue.Crawler.HeatTolerance;
            var byFacing = Enumerable.Range(0, sun.Facings).Select(f => string.Join(",", sun.OpenRoutes(f, tolerance))).ToList();

            Assert.That(byFacing.Distinct().Count(), Is.GreaterThan(1), string.Join(" | ", byFacing));
            Assert.That(sun.OpenRoutes(0, tolerance), Does.Not.Contain("nest_duct"), "at facing 0 the nest is too hot to pass");
            Assert.That(sun.OpenRoutes(1, tolerance), Contains.Item("nest_duct"));
            Assert.That(sun.OpenRoutes(2, _c.Prologue.Spitter.HeatTolerance), Is.Not.Empty, "Spitters take routes Crawlers can't");
        }

        // ---- Broker loans -----------------------------------------------------------------------

        [Test]
        public void ALoan_IsRepaidWithAProBonoFavour_OneAtATime()
        {
            var standing = _t.Factions.NewStanding();
            var synth = _c.Loans.Single(l => l.Id == "loan_military_synth");

            Assert.That(synth.UnitLent, Is.SameAs(_c.Picket));
            Assert.That(synth.UnitMark, Is.EqualTo(Mark.MkIII));
            Assert.That(synth.Favour.IsProBono, Is.True);

            Assert.That(synth.Borrow(standing), Is.True);
            Assert.That(_c.Loans.Any(l => l.CanBorrow(standing)), Is.False, "one loan at a time");

            synth.Favour.Complete(standing);
            Assert.That(standing.Scrip, Is.Zero, "the favour pays nothing");
            Assert.That(_c.Loans.All(l => l.CanBorrow(standing)), Is.True, "the favour clears the debt");
        }

        [Test]
        public void Loans_AreClosedToEnemiesOfTheBroker()
        {
            var standing = _t.Factions.NewStanding();
            _t.Ledger.Choose("lane", standing);

            Assert.That(_c.Loans.Any(l => l.CanBorrow(standing)), Is.False);
        }
    }
}
