using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>Mission 2 (Cold Sleep and the Judea) against the doc's tuning targets. In memory; no assets written.</summary>
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
            Assert.That(_c.Picket.StartingItems.Single(), Is.SameAs(_c.Prologue.ServicePistol), "Picket carries the standard military handgun");
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
        public void Contract_PaysHalfPerTrip_AndRewardsAnAugmentBlueprint()
        {
            Assert.That(_c.Contract.Parts, Is.EqualTo(2));
            Assert.That(_c.Contract.TotalPay, Is.EqualTo(1200));
            Assert.That(_c.Contract.PayAfter(1), Is.EqualTo(600));
            Assert.That(_c.Contract.ModuleGiven.OversizeLoadCapacity, Is.EqualTo(1), "one pod per trip is why it's two trips");
            Assert.That(_c.Contract.ItemsGiven.Single().count, Is.EqualTo(2));
            Assert.That(_c.Contract.CompletionBlueprints, Has.Member(_c.StabilizedArmsBlueprint));
        }

        // ---- Picket's first choice -------------------------------------------------------------

        [Test]
        public void FirstChoice_OnlyBurstGunnerAndBulwarkAreOpen_WithStartingGear()
        {
            var open = _c.Picket.SpecializationOptions.Where(s => s.IsAvailable(_c.Picket.StartingItems)).ToList();

            Assert.That(open, Is.EquivalentTo(new[] { _c.BurstGunner, _c.Bulwark }));
            Assert.That(_c.Marksman.LockedReason, Does.Contain("long rifle"));
            Assert.That(_c.Controller.LockedReason, Is.Not.Empty);
        }

        [Test]
        public void LockedChoices_OpenOnceTheGearExists()
        {
            Assert.That(_c.Marksman.IsAvailable(new ItemDefinition[] { _c.LongRifle }), Is.True);
            Assert.That(_c.Controller.IsAvailable(new ItemDefinition[] { _c.FoamProjector }), Is.True);
        }

        private float SentryEffectiveDps => _c.Prologue.SentryMk1.PeakDamagePerSecond * _c.Prologue.SentryMk1.ExpectedHitRate;

        [Test]
        public void ConversionKit_BeatsTheHandgun_ButNotAMachineGun()
        {
            var pistol = _c.Prologue.ServicePistol.BaseStats;
            var converted = _c.PistolConversionKit.Apply(pistol);

            Assert.That(converted.FireMode, Is.EqualTo(FireMode.Auto));
            Assert.That(converted.MagazineSize, Is.EqualTo(30));
            Assert.That(converted.EffectiveDamagePerSecond, Is.GreaterThan(pistol.EffectiveDamagePerSecond * 1.25f), $"{converted} vs {pistol}");
            Assert.That(converted.EffectiveDamagePerSecond, Is.LessThan(SentryEffectiveDps), $"{converted} vs sentry {SentryEffectiveDps:0.0}");
        }

        [Test]
        public void BurstGunner_IsUsefulWithoutTheKit_AndMatchesASentryWithIt()
        {
            var pistol = _c.Prologue.ServicePistol.BaseStats;
            var bare = pistol.WithUnitModifiers(_c.BurstGunner.Passives);
            var kitted = _c.PistolConversionKit.Apply(pistol).WithUnitModifiers(_c.BurstGunner.Passives);

            Assert.That(bare.EffectiveDamagePerSecond, Is.GreaterThan(pistol.EffectiveDamagePerSecond * 1.3f), $"{bare} vs {pistol}");
            Assert.That(kitted.EffectiveDamagePerSecond, Is.GreaterThanOrEqualTo(SentryEffectiveDps), $"{kitted} vs sentry {SentryEffectiveDps:0.0}");
        }

        [Test]
        public void Bulwark_OutHealsThreeCrawlers_ButNotFour()
        {
            var armour = _c.Picket.Armour + StatMath.SumFlat(UnitStat.Armour, _c.Bulwark.Passives);
            var perCrawler = _c.Prologue.Crawler.Attack.DamagePerSecond * ArmourMath.DamageMultiplier(armour);
            var regen = StatMath.SumFlat(UnitStat.HealthRegenPerSecond, _c.Bulwark.Passives);

            Assert.That(regen, Is.GreaterThan(3 * perCrawler), $"regen {regen}/s vs {3 * perCrawler:0.0}/s");
            Assert.That(regen, Is.LessThan(4 * perCrawler), "four Crawlers should still wear him down");
        }

        [Test]
        public void Picket_CanSpecializeAfterTrip1_EvenWithATenthOfTheStreamKills()
        {
            var guaranteed = _c.Constable.Experience + _c.ConstableNoise.Spawns.Sum(s => s.count * s.alien.Experience);
            var streamXp = _c.Trip1Stream.Spawns.Sum(s => s.count * s.alien.Experience);

            Assert.That(guaranteed + 0.1f * streamXp, Is.GreaterThanOrEqualTo(_c.BurstGunner.UnlockExperience));
        }

        [Test]
        public void Augments_EightSlotsAtTheClinic_PlusACraftOnlyMk2()
        {
            Assert.That(_c.Augments, Has.Count.EqualTo(8));
            Assert.That(_c.Augments.All(_c.Picket.Accepts), Is.True);
            Assert.That(_c.Augments.Select(a => a.Slot).Distinct().Count(), Is.EqualTo(8));
            Assert.That(_c.Prologue.J1M.Accepts(_c.Augments[0]), Is.False, "workers take no augments");

            Assert.That(_c.StabilizedArmsMk2.SoldAtClinic, Is.False);
            Assert.That(_c.StabilizedArmsMk2.Upgrades(_c.Augments.Single(a => a.Slot == AugmentSlot.Arms)), Is.True);
        }

        // ---- The augment hook -------------------------------------------------------------------

        [Test]
        public void ArmsMk2_CantBeMadeAnywhereYet_SoTheNextGoalIsAT2Fabricator()
        {
            var stations = new[] { _c.NellieWorkshop, _c.TorrensFabricator, _c.Prologue.BayFabricator, _c.Prologue.MaintenanceStation };

            Assert.That(_c.StabilizedArmsRecipe.MinimumStationTier, Is.EqualTo(2));
            Assert.That(stations.Any(s => s.CanMake(_c.StabilizedArmsRecipe)), Is.False);
        }

        [Test]
        public void ArmsMk2_PrerequisitesCanAllBeGathered()
        {
            var recipe = _c.StabilizedArmsRecipe;
            Assert.That(_c.ConstableRemains.AmountOf(_c.SyntheticMyomer), Is.GreaterThanOrEqualTo(recipe.AmountOf(_c.SyntheticMyomer)));
            Assert.That(recipe.ItemInputs.Single().item, Is.SameAs(_c.Prologue.GravLatticeAlloy), "keep one alloy unit instead of selling it");

            var chunksNeeded = recipe.AmountOf(_c.XenoalloyIngot) * _c.XenoalloyRecipe.AmountOf(_c.AmalgamChunk);
            Assert.That(TotalChunks(), Is.GreaterThanOrEqualTo(chunksNeeded + 3), "the kit's ingot and the augment's four");
        }

        // ---- Amalgam and ammo -----------------------------------------------------------------

        private static float Weight(WaveDefinition wave, string prefix = null) =>
            wave.Spawns.Where(s => prefix == null || s.spawnPointId.StartsWith(prefix, StringComparison.Ordinal))
                .Sum(s => s.count * s.alien.AmalgamWeight);

        private int Trip1Chunks => _c.Amalgam.DepositsFrom(Weight(_c.Trip1Stream, ColdSleepContentBuilder.LanePrefix), ColdSleepContentBuilder.LaneSegments);

        private int Trip2Chunks => _c.Amalgam.DepositsFrom(
            Weight(_c.Trip2Stream, ColdSleepContentBuilder.LanePrefix) + Weight(_c.Trip2LeaperIntro, ColdSleepContentBuilder.LanePrefix),
            ColdSleepContentBuilder.LaneSegments);

        private int JudeaChunks => _c.Amalgam.DepositsFrom(Weight(_c.JudeaWaves), ColdSleepContentBuilder.JudeaKillZoneSegments);

        private int TotalChunks() => Trip1Chunks + Trip2Chunks + JudeaChunks;

        private float LaneRounds(WaveDefinition wave)
        {
            var sentry = _c.Prologue.SentryMk1;
            return wave.Spawns
                .Where(s => s.spawnPointId.StartsWith(ColdSleepContentBuilder.LanePrefix, StringComparison.Ordinal))
                .Sum(s => s.count * sentry.ExpectedRoundsToKill(s.alien.MaxHealth, s.alien.Armour));
        }

        private float RoundsPerScrap => (float)_c.Prologue.SentryAmmoBox.Rounds / _c.SalvageAmmoRecipe.AmountOf(_c.Prologue.Scrap);

        [Test]
        public void Judea_YieldsEnoughAmalgamForTheKitsIngot()
        {
            Assert.That(JudeaChunks, Is.GreaterThanOrEqualTo(_c.XenoalloyRecipe.AmountOf(_c.AmalgamChunk)), $"{JudeaChunks} chunks");
            Assert.That(_c.Amalgam.YieldOf(_c.Prologue.Circuitry, SurfaceKind.Conduit), Is.EqualTo(1), "conduit amalgam gives Circuitry");
        }

        [TestCase(1)]
        [TestCase(2)]
        public void KillZones_ReturnAThirdToAHalfOfTheirAmmo_NeverFundThemselves(int trip)
        {
            var spent = trip == 1 ? LaneRounds(_c.Trip1Stream) : LaneRounds(_c.Trip2Stream) + LaneRounds(_c.Trip2LeaperIntro);
            var chunks = trip == 1 ? Trip1Chunks : Trip2Chunks;
            var returned = chunks * _c.Amalgam.YieldOf(_c.Prologue.Scrap, SurfaceKind.Bulkhead) * RoundsPerScrap;

            Assert.That(returned / spent, Is.InRange(0.30f, 0.50f), $"trip {trip}: {returned:0} of {spent:0} rounds come back");
        }

        [Test]
        public void Trip1_FitsInOneLoadOfBothSentries()
        {
            var rounds = LaneRounds(_c.Trip1Stream);

            Assert.That(rounds, Is.InRange(1300f, 1500f), $"{rounds:0} rounds");
            Assert.That(rounds, Is.LessThan(2 * _c.Prologue.SentryMk1.AmmoCapacity));
        }

        [Test]
        public void Trip2_IsHeavier_AndNeedsFeedingMidStream()
        {
            var rounds = LaneRounds(_c.Trip2Stream) + LaneRounds(_c.Trip2LeaperIntro);

            Assert.That(rounds, Is.InRange(2400f, 2700f), $"{rounds:0} rounds");
            Assert.That(rounds, Is.GreaterThan(2 * _c.Prologue.SentryMk1.AmmoCapacity));
            Assert.That(_c.Trip2Stream.CountOf(_c.Leaper) + _c.Trip2LeaperIntro.CountOf(_c.Leaper), Is.EqualTo(12));
        }

        [Test]
        public void BulkheadsPlusAmalgamCoverBothTrips_WithAMarginForMistakes()
        {
            var needed = LaneRounds(_c.Trip1Stream) + LaneRounds(_c.Trip2Stream) + LaneRounds(_c.Trip2LeaperIntro);
            var fromBulkheads = _c.BulkheadPanel.TotalAmountOf(_c.Prologue.Scrap) * RoundsPerScrap;
            var fromAmalgam = (Trip1Chunks + Trip2Chunks) * _c.Amalgam.YieldOf(_c.Prologue.Scrap, SurfaceKind.Bulkhead) * RoundsPerScrap;

            Assert.That(fromBulkheads, Is.EqualTo(3500f).Within(0.5f));
            Assert.That(fromBulkheads, Is.LessThan(needed), "bulkheads alone aren't enough: the kill zones must be mined");
            Assert.That(fromBulkheads + fromAmalgam - needed, Is.InRange(800f, 2000f), $"margin {fromBulkheads + fromAmalgam - needed:0} rounds");
        }

        [Test]
        public void ThirdSentry_IsReachableFromLaneAmalgamAndTheStore()
        {
            var lane = Trip1Chunks + Trip2Chunks;
            var bulkheadShare = (float)ColdSleepContentBuilder.LaneBulkheadSegments / ColdSleepContentBuilder.LaneSegments;
            var plating = lane * bulkheadShare * _c.Amalgam.YieldOf(_c.Prologue.Plating, SurfaceKind.Bulkhead);
            var circuitry = lane * (1f - bulkheadShare) * _c.Amalgam.YieldOf(_c.Prologue.Circuitry, SurfaceKind.Conduit)
                            + _c.ConstableRemains.AmountOf(_c.Prologue.Circuitry);
            var ordnance = _c.SecurityStore.AmountOf(_c.Prologue.Ordnance);
            var sentry = _c.Prologue.SentryRecipe;

            Assert.That(plating, Is.GreaterThanOrEqualTo(sentry.AmountOf(_c.Prologue.Plating)), $"plating {plating:0.#}");
            Assert.That(circuitry, Is.GreaterThanOrEqualTo(sentry.AmountOf(_c.Prologue.Circuitry)), $"circuitry {circuitry:0.#}");
            Assert.That(ordnance, Is.GreaterThanOrEqualTo(sentry.AmountOf(_c.Prologue.Ordnance)));
            Assert.That(_c.NellieWorkshop.Recipes, Has.Member(sentry));
        }

        // ---- Noise and Omens ------------------------------------------------------------------

        [Test]
        public void Insulation_ContainsPodDetaching()
        {
            Assert.That(_c.AcousticInsulation.IsContained(_c.PodMount.Noise, _c.PodMount.Vibration), Is.True);
            Assert.That(_c.SecurityStore.AmountOf(_c.AcousticPanel), Is.EqualTo(_c.AcousticInsulation.PanelsRequired));
            Assert.That(_c.AcousticInsulation.InstallSeconds(2), Is.EqualTo(_c.AcousticInsulation.InstallSeconds(1) / 2f));
        }

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
            Assert.That(_c.TheGhost.ValueOf(OmenEffectKind.ContractPayPercent), Is.GreaterThan(_c.ThickHide.BrutalityPayoutPercent));
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
