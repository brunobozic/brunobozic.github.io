using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>The tarot rollout for missions 1–10 against the "Tarot rollout" tab of the design doc.</summary>
    public sealed class OmenRolloutTests
    {
        private InMemoryDefinitionFactory _factory;
        private OmenRolloutContent _r;
        private OmenRolloutDefinition Rollout => _r.Rollout;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _r = OmenRolloutBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        private static Func<string, bool> Path(params string[] flags)
        {
            var set = new HashSet<string>(flags, StringComparer.Ordinal);
            return set.Contains;
        }

        private static readonly Func<string, bool> Lane = Path(OmenRolloutBuilder.LedgerSoldFlag);
        private static readonly Func<string, bool> HollowMoon = Path(OmenRolloutBuilder.LedgerReturnedFlag);

        [Test]
        public void Catalog_HasNoValidationErrors()
        {
            var errors = _r.Catalog.Validate().Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void Schedule_CoversMissionsOneToTen()
        {
            Assert.That(Rollout.Schedule.Select(s => s.mission), Is.EqualTo(Enumerable.Range(1, 10)));
            Assert.That(Rollout.LastMission, Is.EqualTo(10));
        }

        [Test]
        public void Prologue_HasNoSlots_AndOnlyTheFaceDownGleanersMoon()
        {
            var m1 = Rollout.ArrivingIn(1);
            Assert.That(m1.Select(e => e.card), Is.EqualTo(new[] { _r.Previous.Previous.GleanersMoon }));
            Assert.That(m1[0].source, Is.EqualTo(OmenSource.FaceDown));
            Assert.That(Rollout.PlayableIn(1), Is.Empty);
            Assert.That(Rollout.SlotsFor(1, 2, false), Is.Zero);
        }

        [Test]
        public void ColdSleep_TheReaderOpensOmens_WithThreeMajorCards()
        {
            var cs = _r.Previous.Previous;
            Assert.That(Rollout.PlayableIn(2), Is.EquivalentTo(new[] { cs.ThinSwarm, cs.ThickHide, cs.TheGhost }));
            Assert.That(Rollout.PlayableIn(4), Has.Member(cs.GleanersMoon), "the Moon is a Minor: playable once the first Minor slot opens");
            Assert.That(Rollout.SlotsFor(2, 3, false), Is.EqualTo(1));
            Assert.That(Rollout.ArrivingIn(2).Where(e => e.source == OmenSource.Reading).Select(e => e.card),
                Is.EquivalentTo(new[] { cs.ThinSwarm, cs.ThickHide, cs.TheGhost }));
        }

        [Test]
        public void SecondSlot_OpensAfterTheTremolino()
        {
            Rollout.TryGetSchedule(3, out var m3);
            Assert.That(m3.slots, Is.EqualTo(1));
            Assert.That(m3.slotsAfter, Is.EqualTo(2));
            Assert.That(Rollout.SlotsFor(4, 5, false), Is.EqualTo(2));
        }

        [Test]
        public void OneToThreeNewCards_PerMission_AfterTheReading()
        {
            for (var m = 3; m <= 10; m++)
            {
                var count = Rollout.ArrivingIn(m, Lane).Count;
                Assert.That(count, Is.InRange(2, 3), $"mission {m}");
            }
        }

        [Test]
        public void NoRandomDrops_BeforePod7()
        {
            for (var m = 1; m <= 3; m++)
            {
                Assert.That(Rollout.DropPool(m), Is.Empty, $"mission {m}");
            }

            Rollout.TryGetSchedule(4, out var m4);
            Assert.That(m4.dropsMin, Is.EqualTo(1));
            Assert.That(m4.dropsMax, Is.EqualTo(1));
            Assert.That(m4.dropCeiling, Is.EqualTo(Rarity.Common));
        }

        [Test]
        public void Drops_OnlyComeFromCardsAlreadyMet()
        {
            for (var m = 4; m <= 10; m++)
            {
                var metBefore = Rollout.MetBy(m - 1, Lane);
                var pool = Rollout.DropPool(m, Lane);
                Assert.That(pool, Is.Not.Empty, $"mission {m}");
                Assert.That(pool, Is.SubsetOf(metBefore), $"mission {m}");
                Assert.That(pool, Has.None.Matches<OmenCardDefinition>(c => Rollout.ArrivingIn(m, Lane).Any(e => e.card == c)),
                    $"mission {m}: a card arriving now must arrive as an event, not as a drop");
            }
        }

        [Test]
        public void RarityCeilings_RiseOnSchedule()
        {
            var expected = new Dictionary<int, Rarity> { [4] = Rarity.Common, [5] = Rarity.Uncommon, [7] = Rarity.Rare, [10] = Rarity.Epic };
            foreach (var pair in expected)
            {
                Rollout.TryGetSchedule(pair.Key, out var s);
                Assert.That(s.dropCeiling, Is.EqualTo(pair.Value), $"mission {pair.Key}");
            }

            Assert.That(Rollout.Schedule.Max(s => s.dropCeiling), Is.LessThan(Rarity.Legendary), "Legendary waits for Act 3");
        }

        [Test]
        public void TheSephora_GuaranteesTheFirstRare_TheWatcher()
        {
            Rollout.TryGetSchedule(8, out var m8);
            Assert.That(m8.guaranteesRarity, Is.True);
            Assert.That(m8.guaranteedRarity, Is.EqualTo(Rarity.Rare));
            Assert.That(_r.TheWatcher.Rarity, Is.EqualTo(Rarity.Rare));
            Assert.That(Rollout.ArrivingIn(8).Select(e => e.card), Has.Member(_r.TheWatcher));
            Assert.That(Rollout.MetBy(7, Lane).Max(c => c.Rarity), Is.LessThanOrEqualTo(Rarity.Uncommon), "no Rare card is handed out before the Sephora");
        }

        [Test]
        public void TheCartomancersTable_OpensInMissionSix_UpToRare_ThenEpicAtTen()
        {
            Assert.That(Rollout.Schedule.Where(s => s.mission < 6), Has.All.Matches<MissionOmenSchedule>(s => !s.cartomancerOpen));
            Rollout.TryGetSchedule(6, out var m6);
            Rollout.TryGetSchedule(10, out var m10);
            Assert.That(m6.cartomancerOpen, Is.True);
            Assert.That(m6.ascendCeiling, Is.EqualTo(Rarity.Rare));
            Assert.That(m10.ascendCeiling, Is.EqualTo(Rarity.Epic));
        }

        [TestCase(6, false)]
        [TestCase(7, true)]
        public void ThirdSlot_NeedsRankSeven(int rank, bool expectThree)
        {
            Assert.That(Rollout.SlotsFor(6, rank, false), Is.EqualTo(expectThree ? 3 : 2));
        }

        [Test]
        public void RankSeven_IsReachedByMissionSix_AtThePlannedXp()
        {
            var p = _r.Previous.Previous.Prologue;
            var cs = _r.Previous.Previous;
            var xp = p.Mission.CommanderXp + cs.Trip1.CommanderXp + cs.Trip2.CommanderXp + _r.Previous.Mission.CommanderXp;
            Assert.That(p.Ranks.RankAt(xp), Is.LessThan(Rollout.ThirdSlotRank), "not before mission 4");

            xp += 2 * OmenRolloutBuilder.PlannedXpPerMission;
            Assert.That(p.Ranks.RankAt(xp), Is.GreaterThanOrEqualTo(Rollout.ThirdSlotRank), $"by mission 6 ({xp} XP), even without the Judea");
        }

        [Test]
        public void FourthSlot_ComesOnlyFromTheReliquarianNode()
        {
            Assert.That(Rollout.SlotsFor(10, 9, false), Is.EqualTo(3));
            Assert.That(Rollout.SlotsFor(10, 9, true), Is.EqualTo(OmenRolloutDefinition.MaxSlots));
            Assert.That(Rollout.SlotsFor(4, 9, true), Is.EqualTo(2), "the node adds a 4th, it doesn't skip the 3rd");
        }

        [Test]
        public void TheLedger_ChoosesTheMissionSixReward()
        {
            // Mission 3's ledger reward (a Minor) is checked in OmenArcanaTests.
            var lane = Rollout.ArrivingIn(6, Lane).Select(e => e.card).ToList();
            var hollow = Rollout.ArrivingIn(6, HollowMoon).Select(e => e.card).ToList();
            var kept = Rollout.ArrivingIn(6, Path(OmenRolloutBuilder.LedgerKeptFlag)).Select(e => e.card).ToList();

            Assert.That(lane, Is.EquivalentTo(new[] { _r.BrittleWelds, _r.IronChitin }));
            Assert.That(hollow, Is.EquivalentTo(new[] { _r.BrittleWelds, _r.TheDeck }));
            Assert.That(kept, Is.EquivalentTo(new[] { _r.BrittleWelds, _r.TheDeck }));
            Assert.That(Rollout.ArrivingIn(6), Is.EquivalentTo(Rollout.ArrivingIn(6).Where(e => !e.IsPathBound)), "no path, no path card");
        }

        [Test]
        public void ByMissionTen_AboutTwentyFiveCards_AcrossTheFamiliesAndFourSuits()
        {
            var met = Rollout.MetBy(10, Lane);
            Assert.That(met.Count, Is.InRange(22, 26));
            Assert.That(met.Select(c => c.Family).Distinct().Count(), Is.GreaterThanOrEqualTo(10));
            Assert.That(met.Where(c => c.Arcana == OmenArcana.Minor).Select(c => c.Suit).Distinct().Count(), Is.EqualTo(4));
        }

        [Test]
        public void EachFamily_IsIntroducedByACommonCard()
        {
            var firstByFamily = Rollout.Entries.OrderBy(e => e.mission).GroupBy(e => e.card.Family).Select(g => g.First().card);
            Assert.That(firstByFamily, Has.All.Matches<OmenCardDefinition>(c => c.Rarity == Rarity.Common));
        }

        [Test]
        public void CullingCards_TargetTheirAliens()
        {
            Assert.That(_r.CullingLeaper.CulledAlien, Is.SameAs(_r.Previous.Previous.Leaper));
            Assert.That(_r.CullingSpitter.CulledAlien, Is.SameAs(_r.Previous.Previous.Prologue.Spitter));
            Assert.That(_r.CullingLeaper.ValueOf(OmenEffectKind.CulledSpawnReduction), Is.EqualTo(0.6f).Within(1e-5f));
            Assert.That(_r.CullingLeaper.Threat, Is.LessThan(0), "culling makes a run easier");
        }

        [Test]
        public void BloodPrice_IsTheFinalesHighRiskCard()
        {
            var rest = Rollout.MetBy(10, Lane).Where(c => c != _r.BloodPrice);
            Assert.That(_r.BloodPrice.Threat, Is.GreaterThan(rest.Max(c => c.Threat)));
            Assert.That(_r.BloodPrice.BrutalityPayoutPercent, Is.GreaterThan(_r.IronChitin.BrutalityPayoutPercent));
        }

        [TestCase(1, 5, false)]
        [TestCase(2, 1, false)]
        [TestCase(2, 2, true)]
        [TestCase(9, 3, true)]
        public void Mercy_ThinSwarmAfterTwoFailures_FromMissionTwo(int mission, int failures, bool expected)
        {
            Assert.That(Rollout.MercyAvailable(mission, failures), Is.EqualTo(expected));
            Assert.That(Rollout.MercyCard, Is.SameAs(_r.Previous.Previous.ThinSwarm));
        }

        [Test]
        public void CountermeasureHand_GrowsAlongside()
        {
            Assert.That(Rollout.Schedule.Select(s => s.countermeasureHand), Is.EqualTo(new[] { 1, 3, 4, 4, 5, 5, 6, 6, 7, 7 }));
        }

        [Test]
        public void Validation_CatchesABrokenSchedule()
        {
            var bad = _factory.GetOrCreate<OmenRolloutDefinition>("rollout_bad", "Omens");
            bad.id = "rollout_bad";
            bad.displayName = "Bad";
            bad.mercyCard = _r.Previous.Previous.ThinSwarm;
            var good = Rollout.Schedule.ToArray();
            good[5].slots = 1; // mission 6 shrinks below mission 5's two slots
            bad.schedule = good;
            bad.entries = new[]
            {
                new OmenRolloutEntry(4, _r.IronVow, OmenSource.Reward, 5),
                new OmenRolloutEntry(5, _r.IronVow, OmenSource.Reward, 6),
                new OmenRolloutEntry(1, _r.HungryHive, OmenSource.FaceDown, 1)
            };

            var report = new ValidationReport();
            bad.Validate(report);
            var errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.Message).ToList();
            Assert.That(errors, Has.Some.Contains("fewer slots"));
            Assert.That(errors, Has.Some.Contains("more than once"));
            Assert.That(errors, Has.Some.Contains("face-down"));
        }
    }
}
