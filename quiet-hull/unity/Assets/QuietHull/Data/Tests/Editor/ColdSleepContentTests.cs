using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>Mission 2 (Cold Sleep) against the doc's tuning targets. In memory; no assets written.</summary>
    public sealed class ColdSleepContentTests
    {
        private InMemoryDefinitionFactory _factory;
        private ColdSleepContent _c;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _c = ColdSleepContentBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void Catalog_HasNoValidationErrors()
        {
            var errors = _c.Catalog.Validate().Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void SharedDefinitions_AreTheSameObjectsAsThePrologue()
        {
            Assert.That(_c.Contract.ItemsGiven[0].item, Is.SameAs(_c.Prologue.SentryMk1));
            Assert.That(_c.SalvageAmmoRecipe.Output, Is.SameAs(_c.Prologue.SentryAmmoBox));
        }

        // ---- Economy -------------------------------------------------------------------------

        private int RequiredCost => _c.NewIdentity.PriceScrip + _c.TransponderReflash.PriceScrip;

        [Test]
        public void MinimumPath_AffordsTheRequiredBuys_ThenOneAugmentAfterTrip1()
        {
            var arrive = _c.Prologue.GravLatticeAlloy.TotalScripValue(2); // 2 alloy, no crates
            var afterRequired = arrive - RequiredCost;

            Assert.That(afterRequired, Is.GreaterThanOrEqualTo(0), "the story must never soft-lock");
            Assert.That(_c.PicketOffer.LentForContract, Is.True, "Picket must be available without money");
            Assert.That(afterRequired + _c.Contract.PayAfter(1), Is.GreaterThanOrEqualTo(ColdSleepContentBuilder.AugmentPrice));
        }

        [Test]
        public void GreedyPath_AffordsEverythingTheDocLists()
        {
            var arrive = _c.Prologue.GravLatticeAlloy.TotalScripValue(6) + 3 * 100; // 6 alloy, 3 crates
            var shopping = RequiredCost + _c.RepairJ1M.PriceScrip + _c.SecondWorkerOffer.PriceScrip + _c.PicketOffer.PriceScrip + 2 * ColdSleepContentBuilder.AugmentPrice;

            Assert.That(arrive, Is.EqualTo(3100));
            Assert.That(arrive - shopping, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Contract_PaysHalfPerTrip_AndGivesTheCradleAndTwoSentries()
        {
            Assert.That(_c.Contract.Parts, Is.EqualTo(2));
            Assert.That(_c.Contract.TotalPay, Is.EqualTo(1200));
            Assert.That(_c.Contract.PayAfter(1), Is.EqualTo(600));
            Assert.That(_c.Contract.ModuleGiven.OversizeLoadCapacity, Is.EqualTo(1), "one pod per trip is why it's two trips");
            Assert.That(_c.Contract.ItemsGiven.Single().count, Is.EqualTo(2));
            Assert.That(_c.PodMount.PlacedCount, Is.EqualTo(_c.Contract.Parts));
        }

        // ---- Ammo ----------------------------------------------------------------------------

        private float LaneRounds(WaveDefinition wave)
        {
            var sentry = _c.Prologue.SentryMk1;
            return wave.Spawns
                .Where(s => s.spawnPointId.StartsWith(ColdSleepContentBuilder.LanePrefix, StringComparison.Ordinal))
                .Sum(s => s.count * sentry.ExpectedRoundsToKill(s.alien.MaxHealth, s.alien.Armour));
        }

        private int RoundsAvailableFromBulkheads()
        {
            var scrap = _c.BulkheadPanel.TotalAmountOf(_c.Prologue.Scrap);
            var boxes = scrap / _c.SalvageAmmoRecipe.AmountOf(_c.Prologue.Scrap);
            return boxes * _c.Prologue.SentryAmmoBox.Rounds;
        }

        [Test]
        public void Trip1_FitsInOneLoadOfBothSentries()
        {
            var rounds = LaneRounds(_c.Trip1Stream);
            var oneLoad = 2 * _c.Prologue.SentryMk1.AmmoCapacity;

            Assert.That(rounds, Is.InRange(1300f, 1500f), $"{rounds:0} rounds");
            Assert.That(rounds, Is.LessThan(oneLoad));
        }

        [Test]
        public void Trip2_NeedsAReloadMidStream()
        {
            var rounds = LaneRounds(_c.Trip2Stream) + LaneRounds(_c.Trip2LeaperIntro);
            var oneLoad = 2 * _c.Prologue.SentryMk1.AmmoCapacity;

            Assert.That(rounds, Is.InRange(2000f, 2300f), $"{rounds:0} rounds");
            Assert.That(rounds, Is.GreaterThan(oneLoad));
        }

        [Test]
        public void BulkheadsCoverBothTrips_WithAMarginForMistakes()
        {
            var needed = LaneRounds(_c.Trip1Stream) + LaneRounds(_c.Trip2Stream) + LaneRounds(_c.Trip2LeaperIntro);
            var available = RoundsAvailableFromBulkheads();

            Assert.That(available, Is.EqualTo(5000));
            Assert.That(available - needed, Is.InRange(1000f, 2000f), $"margin {available - needed:0} rounds");
        }

        [Test]
        public void Trip2_HasTwelveLeapers()
        {
            Assert.That(_c.Trip2Stream.CountOf(_c.Leaper) + _c.Trip2LeaperIntro.CountOf(_c.Leaper), Is.EqualTo(12));
        }

        // ---- Picket ---------------------------------------------------------------------------

        [Test]
        public void Picket_CanSpecializeAfterTrip1_EvenWithATenthOfTheStreamKills()
        {
            var guaranteed = _c.Constable.Experience + _c.ConstableNoise.Spawns.Sum(s => s.count * s.alien.Experience);
            var streamXp = _c.Trip1Stream.Spawns.Sum(s => s.count * s.alien.Experience);

            Assert.That(guaranteed + 0.1f * streamXp, Is.GreaterThanOrEqualTo(_c.Marksman.UnlockExperience));
        }

        [Test]
        public void Picket_OffersThreeArchetypes_AndAcceptsAllEightAugments()
        {
            Assert.That(_c.Picket.SpecializationOptions.Select(s => s.Archetype),
                Is.EquivalentTo(new[] { Archetype.Marksman, Archetype.Controller, Archetype.Bulwark }));
            Assert.That(_c.Augments, Has.Count.EqualTo(8));
            Assert.That(_c.Augments.All(_c.Picket.Accepts), Is.True);
            Assert.That(_c.Augments.Select(a => a.Slot).Distinct().Count(), Is.EqualTo(8));
            Assert.That(_c.Prologue.J1M.Accepts(_c.Augments[0]), Is.False, "workers take no augments");
        }

        [Test]
        public void Marksman_OneShotsACrawler_AndTwoCritsKillALeaper()
        {
            var rifle = _c.LongRifle;
            var crit = StatMath.SumFlat(UnitStat.CritMultiplier, _c.Marksman.Passives);

            Assert.That(_c.Prologue.Crawler.DamageTaken(rifle.DamagePerShot), Is.GreaterThanOrEqualTo(_c.Prologue.Crawler.MaxHealth));
            Assert.That(_c.Leaper.DamageTaken(rifle.DamagePerShot * crit), Is.GreaterThanOrEqualTo(_c.Leaper.MaxHealth),
                "a crit during the 1 s crouch kills a Leaper");
        }

        [Test]
        public void Controller_CantKill_ButSlowsAndStopsJumps()
        {
            Assert.That(_c.FoamProjector.DamagePerShot, Is.Zero);
            Assert.That(_c.FoamProjector.SlowFraction, Is.GreaterThan(0f));
            Assert.That(_c.Controller.AbilityDescription, Does.Contain("can't jump"));
        }

        [Test]
        public void Bulwark_OutHealsAPackOfThreeCrawlers()
        {
            var armour = _c.Picket.Armour + StatMath.SumFlat(UnitStat.Armour, _c.Bulwark.Passives);
            var incoming = 3 * _c.Prologue.Crawler.Attack.DamagePerSecond * ArmourMath.DamageMultiplier(armour);
            var regen = StatMath.SumFlat(UnitStat.HealthRegenPerSecond, _c.Bulwark.Passives);

            Assert.That(regen, Is.GreaterThan(incoming), $"regen {regen}/s vs {incoming:0.0}/s incoming");
            Assert.That(regen, Is.LessThan(incoming * 4f / 3f), "four Crawlers should still wear him down");
        }

        // ---- Noise ----------------------------------------------------------------------------

        [Test]
        public void Insulation_ContainsPodDetaching()
        {
            Assert.That(_c.AcousticInsulation.IsContained(_c.PodMount.Noise, _c.PodMount.Vibration), Is.True);
            Assert.That(_c.SecurityStore.AmountOf(_c.AcousticPanel), Is.EqualTo(_c.AcousticInsulation.PanelsRequired));
            Assert.That(_c.AcousticInsulation.InstallSeconds(2), Is.EqualTo(_c.AcousticInsulation.InstallSeconds(1) / 2f));
        }

        // ---- Omens ----------------------------------------------------------------------------

        [Test]
        public void StarterOmens_MatchTheDoc()
        {
            Assert.That(_c.ThinSwarm.Threat, Is.EqualTo(-3));
            Assert.That(RarityMath.RoundPercent(100f * _c.ThinSwarm.ValueOf(OmenEffectKind.SurgeSizePercent)), Is.EqualTo(30));
            Assert.That(_c.ThinSwarm.ValueOf(OmenEffectKind.LootPercent), Is.EqualTo(-25f));

            Assert.That(_c.ThickHide.Threat, Is.EqualTo(2));
            Assert.That(_c.ThickHide.BrutalityPayoutPercent, Is.EqualTo(8f));

            Assert.That(_c.TheGhost.Threat, Is.EqualTo(0));
            Assert.That(_c.TheGhost.IsVow, Is.True);
            Assert.That(_c.TheGhost.ValueOf(OmenEffectKind.LootPercent), Is.EqualTo(30f));
            Assert.That(_c.TheGhost.BrokenValueOf(OmenEffectKind.AwarenessJumpPercent), Is.EqualTo(20f));
        }

        [Test]
        public void TheGhost_PaysMoreThanThickHide_ForTheExtraSkill()
        {
            var ghostPay = _c.TheGhost.ValueOf(OmenEffectKind.ContractPayPercent);
            var thickPay = _c.ThickHide.BrutalityPayoutPercent;

            Assert.That(ghostPay, Is.GreaterThan(thickPay));
        }

        [TestCase(Rarity.Rare, 60f, 40f, 30f)]
        [TestCase(Rarity.Legendary, 90f, 60f, 40f)]
        public void TheGhost_ScalesWithRarity(Rarity rarity, float loot, float pay, float penalty)
        {
            _c.TheGhost.rarity = rarity;

            Assert.That(_c.TheGhost.ValueOf(OmenEffectKind.LootPercent), Is.EqualTo(loot).Within(1e-3f));
            Assert.That(_c.TheGhost.ValueOf(OmenEffectKind.ContractPayPercent), Is.EqualTo(pay).Within(1e-3f));
            Assert.That(_c.TheGhost.BrokenValueOf(OmenEffectKind.AwarenessJumpPercent), Is.EqualTo(penalty).Within(1e-3f));
        }
    }
}
