using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>
    /// Checks the prologue data against the design doc's tuning targets. These run in memory:
    /// no assets are written.
    /// </summary>
    public sealed class PrologueContentTests
    {
        private InMemoryDefinitionFactory _factory;
        private PrologueContent _content;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _content = PrologueContentBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void Catalog_HasNoValidationErrors()
        {
            var report = _content.Catalog.Validate();

            var errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void Catalog_IdsAreUniqueAndResolvable()
        {
            var ids = _content.All.Select(d => d.Id).ToList();

            Assert.That(ids, Is.Unique);
            foreach (var id in ids)
            {
                Assert.That(_content.Catalog.TryGet<DefinitionBase>(id, out _), Is.True, id);
            }
        }

        [Test]
        public void Building_Twice_ReusesInstances()
        {
            var again = PrologueContentBuilder.Build(_factory);

            Assert.That(again.ShockProd, Is.SameAs(_content.ShockProd));
            Assert.That(again.Catalog.Definitions.Count, Is.EqualTo(_content.Catalog.Definitions.Count));
        }

        [Test]
        public void Mission_Has27OrderedBeatsAnd7Checkpoints()
        {
            var beats = _content.Mission.Beats;

            Assert.That(beats.Count, Is.EqualTo(27));
            Assert.That(beats.Select(b => b.Order), Is.Ordered.Ascending);
            Assert.That(beats.Select(b => b.TargetCountdownSeconds), Is.Ordered.Descending);
            Assert.That(beats.Count(b => b.IsCheckpoint), Is.EqualTo(7));
            Assert.That(_content.Mission.CountdownSeconds, Is.EqualTo(1800f));
        }

        [Test]
        public void ShockProd_KillsACrawlerInTwoHits()
        {
            var perHit = _content.Crawler.DamageTaken(_content.ShockProd.Damage);

            Assert.That(Math.Ceiling(_content.Crawler.MaxHealth / perHit), Is.EqualTo(2));
        }

        [Test]
        public void Senses_SpitterSeesHeat_CrawlerDoesNot()
        {
            Assert.That(_content.Spitter.HasHeatVision, Is.True);
            Assert.That(_content.Crawler.HasHeatVision, Is.False);
            Assert.That(_content.Crawler.CanSignal, Is.True);
            Assert.That(_content.Spitter.DefaultBehaviour, Is.EqualTo(AlienBehaviour.HoldPosition));
        }

        [Test]
        public void Cart_IsMaskedInTheCrusherBay_Always()
        {
            var noise = _content.HandCart.NoiseMoving;

            for (var t = 0f; t < 20f; t += 0.5f)
            {
                Assert.That(_content.CrusherBay.IsMasked(noise, t), Is.True, $"t={t}");
            }
        }

        [Test]
        public void Cart_InTheValveGallery_IsMaskedOnlyDuringABurst()
        {
            var noise = _content.HandCart.NoiseMoving;

            Assert.That(_content.ValveGallery.IsMasked(noise, 1f), Is.True, "during the burst");
            Assert.That(_content.ValveGallery.IsMasked(noise, 6f), Is.False, "between bursts");
            Assert.That(_content.ValveGallery.IsMasked(_content.StunGrenade.NoiseOnUse, 1f), Is.False, "a grenade is louder than the vents");
        }

        [TestCase(0, float.PositiveInfinity)]
        [TestCase(1, 100f)]
        [TestCase(2, 100f / 3f)]
        public void CryoCell_RuptureTimeByHeat(int heat, float expectedSeconds)
        {
            Assert.That(_content.CryoCell.SecondsToRupture(heat), Is.EqualTo(expectedSeconds).Within(0.01f));
        }

        [Test]
        public void FuelCylinder_IgnoresHeat()
        {
            Assert.That(_content.FuelCylinder.SecondsToRupture(3), Is.EqualTo(float.PositiveInfinity));
        }

        [TestCase(0, 0)]
        [TestCase(2, 800)]
        [TestCase(4, 1600)]
        [TestCase(5, 2200)]
        [TestCase(6, 2800)]
        public void Alloy_ValueRewardsGreed(int units, int expectedScrip)
        {
            Assert.That(_content.GravLatticeAlloy.TotalScripValue(units), Is.EqualTo(expectedScrip));
        }

        [Test]
        public void Cart_HoldsTheWholeFuelRunAndAllSixAlloyUnits()
        {
            var fuelRun = 2 * _content.FuelCylinder.Slots + 2 * _content.CryoCell.Slots;

            Assert.That(fuelRun, Is.LessThanOrEqualTo(_content.HandCart.CapacitySlots));
            Assert.That(6 * _content.GravLatticeAlloy.SlotsPerUnit, Is.LessThanOrEqualTo(_content.HandCart.CapacitySlots));
        }

        [Test]
        public void Cage_CoversTwoSentriesAndFourAmmoBoxes_WithTwoOrdnanceSpareForAGrenade()
        {
            var cage = _content.QuartermasterCage;
            var needed = new Dictionary<MaterialDefinition, int>();

            void Add(RecipeDefinition recipe, int times)
            {
                foreach (var cost in recipe.Inputs)
                {
                    needed.TryGetValue(cost.material, out var current);
                    needed[cost.material] = current + cost.amount * times;
                }
            }

            Add(_content.SentryRecipe, 2);
            Add(_content.AmmoBoxRecipe, 4);

            Assert.That(cage.AmountOf(_content.Plating), Is.EqualTo(needed[_content.Plating]));
            Assert.That(cage.AmountOf(_content.Circuitry), Is.EqualTo(needed[_content.Circuitry]));

            var spareOrdnance = cage.AmountOf(_content.Ordnance) - needed[_content.Ordnance];
            Assert.That(spareOrdnance, Is.EqualTo(_content.StunGrenadeRecipe.AmountOf(_content.Ordnance)));
        }

        [Test]
        public void Ammo_FourBoxesFillTwoSentries()
        {
            Assert.That(4 * _content.SentryAmmoBox.Rounds, Is.EqualTo(2 * _content.SentryMk1.AmmoCapacity));
        }

        [Test]
        public void Shelves_HoldExactlyTheScrapForTheCart()
        {
            Assert.That(_content.MaintenanceShelves.AmountOf(_content.Scrap), Is.EqualTo(_content.HandCartRecipe.AmountOf(_content.Scrap)));
        }

        [TestCase(PrologueContentBuilder.LaneAPrefix)]
        [TestCase(PrologueContentBuilder.LaneBPrefix)]
        public void Stream_LeavesEachSentryWith10To20PercentAmmo(string lanePrefix)
        {
            var sentry = _content.SentryMk1;
            var roundsUsed = 0f;

            foreach (var wave in _content.StreamWaves)
            {
                foreach (var spawn in wave.Spawns.Where(s => s.spawnPointId.StartsWith(lanePrefix, StringComparison.Ordinal)))
                {
                    roundsUsed += spawn.count * sentry.ExpectedRoundsToKill(spawn.alien.MaxHealth, spawn.alien.Armour);
                }
            }

            var remaining = 1f - roundsUsed / sentry.AmmoCapacity;
            Assert.That(remaining, Is.InRange(0.10f, 0.20f), $"{lanePrefix}: {roundsUsed:0} rounds used, {remaining:P0} left");
        }

        [Test]
        public void Stream_GantrySpitterIsOutsideTheLanes()
        {
            var gantry = _content.StreamWaves.SelectMany(w => w.Spawns).Where(s => s.alien == _content.Spitter).ToList();

            Assert.That(gantry, Has.Count.EqualTo(1));
            Assert.That(gantry[0].spawnPointId, Does.Not.StartWith(PrologueContentBuilder.LaneAPrefix).And.Not.StartWith(PrologueContentBuilder.LaneBPrefix));
        }
    
        [Test]
        public void Keycard_IsOnOkafor_AndUnlocksTheFloodValve()
        {
            Assert.That(_content.ChessEngineer.Items.Select(i => i.item), Has.Member(_content.ValveKeycard));
            Assert.That(_content.CoolantFlood.RequiresKey, Is.SameAs(_content.ValveKeycard));
            Assert.That(_content.ValveKeycard.OpensDoorId, Is.EqualTo("door_valve_control"));
        }

        [Test]
        public void CoolantFlood_FreezesJ1MBeforeHeCrossesTheRoom()
        {
            const float roomLengthMetres = 20f;
            var speed = _content.J1M.MoveSpeed * _content.CoolantFlood.MoveSpeedMultiplier;
            var secondsToCross = roomLengthMetres / speed;

            Assert.That(_content.CoolantFlood.SecondsToDisable(_content.J1M.MaxHealth), Is.LessThan(secondsToCross));
        }

        [Test]
        public void CoolantJunction_AfterTheValve_IsSafeForTheCryoCells()
        {
            Assert.That(_content.CoolantFlood.ResidualIntensity, Is.EqualTo(1));
            Assert.That(_content.CoolantFlood.ResidualDamagePerSecond, Is.Zero);
            Assert.That(_content.CryoCell.ConditionLossPerSecond(0), Is.Zero, "cold rooms never hurt the cells");
        }

        [Test]
        public void DarkCorridor_LampSeesFurtherButIsSpottedFurther()
        {
            var dark = _content.DarkReactorCorridor;

            Assert.That(_content.J1M.HasLamp, Is.True);
            Assert.That(dark.VisionWithLamp, Is.GreaterThan(dark.VisionWithoutLamp));
            Assert.That(dark.LampSpottedRangeMultiplier, Is.GreaterThan(1f));
            Assert.That(_content.StillCrawlers.CountOf(_content.Crawler), Is.EqualTo(3));
        }

        [Test]
        public void SunEvent_ThawsTheFrozenCrawlers()
        {
            var sun = _content.Mission.Beats.Single(b => b.Id == "beat_15_sun");

            Assert.That(sun.StartsWaves, Has.Member(_content.ThawingCrawlers));
        }

        [Test]
        public void Warden_AnnouncesInCountdownOrder_AndOpensAtThirtyMinutes()
        {
            var cursor = 0;
            var fired = new List<string>();

            for (var t = 1800f; t >= 0f; t -= 1f)
            {
                while (_content.Warden.TryTakeDue(t, ref cursor, out var line))
                {
                    fired.Add(line.text);
                }
            }

            Assert.That(fired.Count, Is.EqualTo(_content.Warden.Announcements.Count));
            Assert.That(fired[0], Does.StartWith("Attention. Reactor containment failure in thirty minutes"));
            Assert.That(_content.Warden.Announcements.Count(a => a.degraded), Is.GreaterThan(0), "the voice should break down");
        }
    }
}
