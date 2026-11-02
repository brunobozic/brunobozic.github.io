using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>Mission 3 (the Tremolino) against the doc: the T2 fabricator, the hive clock, the ledger and the outpost.</summary>
    public sealed class TremolinoTests
    {
        private InMemoryDefinitionFactory _factory;
        private TremolinoContent _t;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _t = TremolinoContentBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void Catalog_HasNoValidationErrors()
        {
            var errors = _t.Catalog.Validate().Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        // ---- New aliens ------------------------------------------------------------------------

        [Test]
        public void Husks_AreFodderForFragMines()
        {
            Assert.That(_t.Previous.Ordnance.Frag.Kills(_t.Husk), Is.True);
            Assert.That(_t.Husk.SurgeCost, Is.EqualTo(1));
        }

        [Test]
        public void Bloats_EatDoors_BurstOnDeath_AndLeaveABigDeposit()
        {
            Assert.That(_t.Bloat.BurstsOnDeath, Is.True);
            Assert.That(_t.Bloat.DeathBurstEatsDoors, Is.True);
            Assert.That(_t.Bloat.DoorDamagePerSecond, Is.GreaterThan(0f));
            Assert.That(_t.Bloat.AmalgamWeight, Is.EqualTo(4f).Within(1e-5f), "a dead Bloat is worth four Crawlers of amalgam");
            Assert.That(_t.Previous.Ordnance.Frag.Kills(_t.Bloat), Is.False, "frag mines don't stop Bloats");
            Assert.That(_t.Bloat.SecondsToBurn(2), Is.EqualTo(float.PositiveInfinity), "Bloats stand Heat 2");
        }

        // ---- The fabricator ---------------------------------------------------------------------

        [Test]
        public void T2Fabricator_MakesTheMk2Arms_OnceTheCoreIsInstalled()
        {
            var fab = _t.T2Fabricator;

            Assert.That(fab.Tier, Is.EqualTo(2));
            Assert.That(fab.CanMake(_t.Previous.ServoArmsMk2Recipe), Is.True);
            Assert.That(fab.Recipes, Has.Member(_t.Previous.ServoArmsMk2Recipe));
            Assert.That(fab.RequiresKey, Is.SameAs(_t.RegulatorCore));
            Assert.That(_t.CryoVault.Items.Select(i => i.item), Has.Member(_t.RegulatorCore));
        }

        [Test]
        public void TheCore_IsOnlyReachableAtFacing2_TheFabricatorAt1To3()
        {
            Assert.That(Enumerable.Range(0, _t.Sun.Facings).Where(f => !_t.Sun.IsSunLocked("cryo_vault", f)), Is.EqualTo(new[] { 2 }));
            Assert.That(Enumerable.Range(0, _t.Sun.Facings).Where(f => !_t.Sun.IsSunLocked("fabricator_t2", f)), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void DeepHold_IsClearedByStrippingTheReactorHousing()
        {
            Assert.That(_t.DeepHoldRadiation.Kind, Is.EqualTo(HazardKind.Radiation));
            Assert.That(_t.DeepHoldRadiation.ClearedAtInteractionId, Is.EqualTo("belt:reactor_housing"));
            Assert.That(_t.Belt.Find("reactor_housing").outcome, Is.EqualTo(ErosionOutcome.StripRadiationSource));
            Assert.That(_t.DeepHold.Items.Sum(i => i.item == _t.Previous.Prologue.GravLatticeAlloy ? i.count : 0), Is.EqualTo(3));
        }

        // ---- The hive ---------------------------------------------------------------------------

        [Test]
        public void Hive_WakesOnTheConsole_HuntsWhenTheNestIsDisturbed_AndNeverCalmsDown()
        {
            var hive = _t.Hive;

            Assert.That(hive.Next(HiveStage.Dormant, 30f, null), Is.EqualTo(HiveStage.Dormant));
            Assert.That(hive.Next(HiveStage.Dormant, 30f, TremolinoContentBuilder.ConsolePoweredEvent), Is.EqualTo(HiveStage.Restless));
            Assert.That(hive.Next(HiveStage.Restless, 400f, TremolinoContentBuilder.NestDisturbedEvent), Is.EqualTo(HiveStage.Hunt));
            Assert.That(hive.Next(HiveStage.Dormant, 601f, null), Is.EqualTo(HiveStage.Restless), "time wakes it too");
            Assert.That(hive.Next(HiveStage.Restless, 1501f, null), Is.EqualTo(HiveStage.Hunt));
            Assert.That(hive.Next(HiveStage.Hunt, 0f, null), Is.EqualTo(HiveStage.Hunt));
            Assert.That(hive.SurgeBudgetPerMinuteAt(HiveStage.Hunt), Is.GreaterThan(hive.SurgeBudgetPerMinuteAt(HiveStage.Restless)));
        }

        // ---- The ledger ---------------------------------------------------------------------------

        private PlayerStanding AfterMission2()
        {
            var standing = _t.Factions.NewStanding();
            foreach (var change in _t.Previous.Contract.CompletionStanding) standing.Apply(change);
            foreach (var change in _t.Previous.JudeaContract.CompletionStanding) standing.Apply(change);
            return standing;
        }

        private int Unlocked(PlayerStanding standing) => _t.Previous.TeaserOffers.Count(o => o.IsUnlockedFor(standing));

        private bool IsUnlocked(PlayerStanding standing, AugmentSlot slot, AugmentLine line) =>
            _t.Previous.TeaserOffers.Single(o => o.Target is BlueprintDefinition bp && bp.Unlocks.Output == _t.Previous.Augs.Synth(slot, line)).IsUnlockedFor(standing);

        [Test]
        public void Mission2_UnlocksTheStandardAndFineLines_ButNoMilitaryOrShadowParts()
        {
            var standing = AfterMission2();

            Assert.That(standing.Of(_t.Factions.HollowMoon), Is.EqualTo(Standing.Friendly));
            Assert.That(standing.Of(_t.Factions.DriftersUnion), Is.EqualTo(Standing.Friendly));
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Standard), Is.True);
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Fine), Is.True);
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Assault), Is.False);
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Shadow), Is.False);
        }

        [Test]
        public void Ledger_ToDoramin_OpensTheBackRoom()
        {
            var standing = AfterMission2();
            var before = Unlocked(standing);

            _t.Ledger.Choose("doramin", standing);

            Assert.That(standing.Of(_t.Factions.HollowMoon), Is.EqualTo(Standing.Trusted));
            Assert.That(standing.Of(_t.Factions.LaneAuthority), Is.EqualTo(Standing.Unfriendly));
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Assault), Is.True, "Assault parts via Hollow Moon: Trusted");
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Shadow), Is.False, "Shadow parts also need the Quiet Choir");
            Assert.That(_t.Previous.TeaserOffers.Single(o => o.Target == _t.Previous.Ordnance.HiddenCompartment).IsUnlockedFor(standing), Is.True);
            Assert.That(Unlocked(standing), Is.GreaterThan(before));
            Assert.That(standing.HasFlag("ledger_returned"), Is.True);
        }

        [Test]
        public void Ledger_ToTheLaneAuthority_PaysAndLicences_ButClosesPatusan()
        {
            var standing = AfterMission2();

            _t.Ledger.Choose("lane", standing);

            Assert.That(standing.Scrip, Is.EqualTo(2000));
            Assert.That(standing.Holds(_t.Factions.LaneMilitaryLicence), Is.True);
            Assert.That(standing.Of(_t.Factions.HollowMoon), Is.EqualTo(Standing.Hostile));
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Assault), Is.True, "Assault parts via the licence");
            Assert.That(IsUnlocked(standing, AugmentSlot.Arms, AugmentLine.Standard), Is.False, "the Hollow Moon no longer deals with you");
            Assert.That(_t.Previous.TeaserOffers.Single(o => o.Target == _t.Previous.Ordnance.HiddenCompartment).IsUnlockedFor(standing), Is.False);
            Assert.That(standing.HasFlag("hollow_moon_reprisal"), Is.True);
        }

        [Test]
        public void Ledger_Kept_ChangesNothingNow()
        {
            var standing = AfterMission2();
            var before = Unlocked(standing);

            _t.Ledger.Choose("keep", standing);

            Assert.That(Unlocked(standing), Is.EqualTo(before));
            Assert.That(standing.HasFlag("ledger_kept"), Is.True);
        }

        // ---- Rewards and the outpost --------------------------------------------------------------

        [Test]
        public void Outpost_LastsSixVisits()
        {
            Assert.That(_t.Outpost.Workstation, Is.SameAs(_t.T2Fabricator));
            Assert.That(_t.Outpost.CanVisit(5), Is.True);
            Assert.That(_t.Outpost.CanVisit(6), Is.False);
        }

        [Test]
        public void OmenRewards_MatchTheDoc()
        {
            Assert.That(_t.SolarFlare.Threat, Is.EqualTo(2));
            Assert.That(_t.SolarFlare.ValueOf(OmenEffectKind.CurrencyPercent), Is.EqualTo(50f));
            Assert.That(_t.QuietDecks.Threat, Is.EqualTo(-3));
            Assert.That(RarityMath.RoundPercent(100f * _t.QuietDecks.ValueOf(OmenEffectKind.PatrolReduction)), Is.EqualTo(50));
        }

        [Test]
        public void Mission_IsParTimed_NotAFailTimer()
        {
            Assert.That(_t.Mission.CountdownIsFailTimer, Is.False);
            Assert.That(_t.Mission.Beats, Has.Count.EqualTo(10));
            Assert.That(_t.Mission.Beats.Select(b => b.TargetCountdownSeconds), Is.Ordered.Descending);
        }
    }
}
