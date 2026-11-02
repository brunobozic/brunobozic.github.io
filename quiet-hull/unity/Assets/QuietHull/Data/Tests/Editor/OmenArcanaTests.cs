using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>
    /// Major and Minor Arcana, card levels and ascending, Catalyst mutations and corruption, the bench Catalysts and
    /// amalgam infusion: the "Inspiration: Martyr tarot, PoE orbs" tab, as decided.
    /// </summary>
    public sealed class OmenArcanaTests
    {
        private InMemoryDefinitionFactory _factory;
        private OmenRolloutContent _r;
        private OmenRolloutDefinition Rollout => _r.Rollout;
        private CartomancerDefinition Table => _r.Cartomancer;
        private ColdSleepContent Cs => _r.Previous.Previous;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _r = OmenRolloutBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        private static OmenCardInstance Card(OmenCardDefinition definition) => new OmenCardInstance(definition);

        private static Func<string, bool> Path(string flag) => f => f == flag;

        private OmenCardInstance LevelFive(OmenCardDefinition definition)
        {
            var card = Card(definition);
            for (var i = 0; i < 10; i++)
            {
                card = Table.AddDuplicate(card, definition, Rarity.Common).Card;
            }

            for (var level = 1; level < RarityMath.MaxCardLevel; level++)
            {
                var result = Table.LevelUp(card, 10_000, false);
                Assert.That(result.Success, Is.True, result.Reason);
                card = result.Card;
            }

            return card;
        }

        // ---- Major and Minor Arcana -------------------------------------------------------------

        [Test]
        public void EveryHulkCard_IsMajor_AndEverySuitCard_IsMinor()
        {
            var all = Rollout.Entries.Select(e => e.card).Distinct().ToList();
            Assert.That(all.Where(c => c.Family != OmenFamily.Suit), Has.All.Matches<OmenCardDefinition>(c => c.Arcana == OmenArcana.Major));
            Assert.That(all.Where(c => c.Family == OmenFamily.Suit), Has.All.Matches<OmenCardDefinition>(c => c.Arcana == OmenArcana.Minor && c.Suit != OmenSuit.None));
            Assert.That(Cs.GleanersMoon.Suit, Is.EqualTo(OmenSuit.Cogs));
        }

        [Test]
        public void Slots_AreOneMajor_ThenMinors_OneTwoThree()
        {
            Assert.That(Rollout.MajorSlotsFor(1, false), Is.Zero);
            Assert.That(Rollout.MajorSlotsFor(2, false), Is.EqualTo(1));
            Assert.That(Rollout.MinorSlotsFor(3, 5, false), Is.Zero);
            Assert.That(Rollout.MinorSlotsFor(4, 5, false), Is.EqualTo(1), "the first Minor slot opens after the Tremolino");
            Assert.That(Rollout.MinorSlotsFor(6, 6, false), Is.EqualTo(1), "the second Minor waits for Rank 7");
            Assert.That(Rollout.MinorSlotsFor(6, 7, false), Is.EqualTo(2));
            Assert.That(Rollout.MinorSlotsFor(10, 9, true), Is.EqualTo(3), "the third Minor is the Reliquarian node");
        }

        [Test]
        public void SetPieces_LockTheMajorSlot_InPatusansFall_TheSephora_AndTheFinale()
        {
            Assert.That(Rollout.Schedule.Where(s => s.hasSetPieceLockingMajor).Select(s => s.mission), Is.EqualTo(new[] { 6, 8, 10 }));
            Rollout.TryGetSchedule(6, out var m6);
            Assert.That(m6.workingTitle, Is.EqualTo("The fall of Patusan"));
            Assert.That(Rollout.MajorSlotsFor(8, true), Is.Zero);
            Assert.That(Rollout.MinorSlotsFor(8, 7, false), Is.EqualTo(2), "Minors stay open on a set-piece");
        }

        [Test]
        public void TheLedger_HandsOutTheFirstMinor_OnePerPath()
        {
            OmenCardDefinition MinorFor(string flag) =>
                Rollout.ArrivingIn(3, Path(flag)).Select(e => e.card).Single(c => c.Arcana == OmenArcana.Minor);

            Assert.That(MinorFor(OmenRolloutBuilder.LedgerSoldFlag).Suit, Is.EqualTo(OmenSuit.Coins));
            Assert.That(MinorFor(OmenRolloutBuilder.LedgerReturnedFlag).Suit, Is.EqualTo(OmenSuit.Seals));
            Assert.That(MinorFor(OmenRolloutBuilder.LedgerKeptFlag).Suit, Is.EqualTo(OmenSuit.Lanterns));
        }

        [Test]
        public void EveryMinor_PushesOneRewardStream_ForAOneThreatHandicap()
        {
            foreach (var minor in _r.MinorCards.Append(Cs.GleanersMoon))
            {
                Assert.That(minor.Threat, Is.EqualTo(1), minor.Id);
                Assert.That(minor.Effects.Count(e => e.side == OmenSide.Handicap), Is.EqualTo(1), minor.Id);
            }

            Assert.That(_r.MinorCards.Append(Cs.GleanersMoon).Select(c => c.Suit).Distinct().Count(), Is.EqualTo(6), "all six suits exist");
        }

        // ---- The loadout ------------------------------------------------------------------------

        [Test]
        public void Loadout_OneMajor_MinorsOfDifferentSuits()
        {
            var legal = new[] { Card(Cs.ThickHide), Card(Cs.GleanersMoon), Card(_r.TheTithe) };
            Assert.That(OmenLoadoutRules.Check(legal, 3, false), Is.Empty);

            var twoMajors = new[] { Card(Cs.ThickHide), Card(Cs.ThinSwarm) };
            Assert.That(OmenLoadoutRules.Check(twoMajors, 3, false), Has.Some.Contains("one Major"));

            var sameSuit = new[] { Card(_r.TheTithe), Card(_r.TheTithe) };
            Assert.That(OmenLoadoutRules.Check(sameSuit, 3, false), Has.Some.Contains("two Minor cards of Coins"));

            var tooManyMinors = new[] { Card(Cs.GleanersMoon), Card(_r.TheTithe) };
            Assert.That(OmenLoadoutRules.Check(tooManyMinors, 2, false), Has.Some.Contains("1 Minor slots open"));

            Assert.That(OmenLoadoutRules.Check(new[] { Card(Cs.ThickHide) }, 3, true), Has.Some.Contains("set-piece"));
        }

        [Test]
        public void Stacking_TheSecondCardOfAFamily_CountsHalf()
        {
            // The Mercy card takes no slot, so it can sit beside a Major of its own family.
            var cards = new[] { Card(Cs.ThinSwarm), Card(_r.Previous.QuietDecks) };
            Assert.That(OmenLoadoutRules.Stacked(cards, OmenEffectKind.LootPercent), Is.EqualTo(-25f - 12.5f).Within(1e-4f));

            var different = new[] { Card(Cs.TheGhost), Card(_r.TheTithe) };
            Assert.That(OmenLoadoutRules.Stacked(different, OmenEffectKind.ContractPayPercent), Is.EqualTo(40f).Within(1e-4f));
        }

        // ---- Levels -----------------------------------------------------------------------------

        [Test]
        public void LevelMath_LevelFive_StaysBelowTheNextRaritysLevelOne()
        {
            for (var r = Rarity.Common; r < Rarity.Legendary; r++)
            {
                Assert.That(RarityMath.M(r, 5), Is.LessThan(RarityMath.M(r + 1, 1)), $"m at {r}");
                Assert.That(RarityMath.H(r, 5), Is.LessThan(RarityMath.H(r + 1, 1)), $"h at {r}");
                Assert.That(RarityMath.M(r, 1), Is.EqualTo(RarityMath.M(r)), "Level I is the plain rarity");
            }

            Assert.That(RarityMath.M(Rarity.Common, 5) - 1f, Is.GreaterThan(RarityMath.H(Rarity.Common, 5) - 1f), "levels raise the benefit faster than the handicap");
        }

        [Test]
        public void Levels_TakeTenCopies_AndFeesOf375()
        {
            var card = Card(Cs.ThickHide);
            var copies = 0;
            var fees = 0;
            for (var level = 1; level < RarityMath.MaxCardLevel; level++)
            {
                copies += Table.CopiesToLevelUp(level, false);
                fees += Table.LevelFee(level);
            }

            Assert.That(copies, Is.EqualTo(10));
            Assert.That(fees, Is.EqualTo(375));

            var five = LevelFive(Cs.ThickHide);
            Assert.That(five.Level, Is.EqualTo(5));
            Assert.That(five.ValueOf(OmenEffectKind.AlienHealthPercent), Is.EqualTo(25f * 1.2f).Within(1e-4f), "h + 0.05 × 4");
            Assert.That(card.Level, Is.EqualTo(1), "the original instance never changes");
        }

        [Test]
        public void QuietChoirTrusted_MakesLevelsThreeToFive_OneCopyCheaper()
        {
            var trusted = Enumerable.Range(1, 4).Sum(l => Table.CopiesToLevelUp(l, true));
            Assert.That(trusted, Is.EqualTo(1 + 1 + 2 + 3));
        }

        [Test]
        public void RarerDuplicates_CountMore_AndOnlyTheSameCardCounts()
        {
            var card = Card(Cs.ThickHide);
            Assert.That(Table.AddDuplicate(card, Cs.ThickHide, Rarity.Uncommon).Card.Progress, Is.EqualTo(2));
            Assert.That(Table.AddDuplicate(card, Cs.ThinSwarm, Rarity.Common).Success, Is.False);
        }

        [Test]
        public void LevelUp_NeedsTheCopiesAndTheFee()
        {
            var card = Card(Cs.ThickHide);
            Assert.That(Table.LevelUp(card, 1000, false).Reason, Does.Contain("duplicate"));
            card = Table.AddDuplicate(card, Cs.ThickHide, Rarity.Common).Card;
            Assert.That(Table.LevelUp(card, 10, false).Reason, Does.Contain("25 Scrip"));
            var ok = Table.LevelUp(card, 25, false);
            Assert.That(ok.Success, Is.True);
            Assert.That(ok.ScripCost, Is.EqualTo(25));
        }

        [Test]
        public void Ascend_OnlyAtLevelFive_UpToTheCeiling_BackToLevelOne()
        {
            Assert.That(Table.Ascend(Card(Cs.ThickHide), 10_000, Rarity.Rare).Success, Is.False);

            var five = LevelFive(Cs.ThickHide);
            Assert.That(Table.Ascend(five, 10_000, Rarity.Common).Success, Is.False, "the ceiling holds");
            Assert.That(Table.Ascend(five, 49, Rarity.Rare).Success, Is.False, "50 Scrip from Common");

            var up = Table.Ascend(five, 50, Rarity.Rare);
            Assert.That(up.Success, Is.True);
            Assert.That(up.Card.Rarity, Is.EqualTo(Rarity.Uncommon));
            Assert.That(up.Card.Level, Is.EqualTo(1));
            Assert.That(up.Card.ValueOf(OmenEffectKind.AlienHealthPercent), Is.GreaterThan(five.ValueOf(OmenEffectKind.AlienHealthPercent)));
            Assert.That(up.Card.BrutalityPayoutPercent, Is.GreaterThan(five.BrutalityPayoutPercent), "ascending is always worth it");
        }

        // ---- Mutations --------------------------------------------------------------------------

        [Test]
        public void Refine_IsALevelWithoutDuplicates()
        {
            var refined = Table.Mutate(Card(Cs.ThickHide), CardMutation.Refine, 0f).Card;
            Assert.That(refined.Level, Is.EqualTo(2));
            Assert.That(CartomancerDefinition.CatalystFor(CardMutation.Refine), Is.EqualTo(CatalystKind.Refine));
        }

        [Test]
        public void Deepen_AddsAHandicap_ThreatPlusTwo_BenefitPlusAQuarter_Once()
        {
            var ghost = Card(Cs.TheGhost);
            var deep = Table.Mutate(ghost, CardMutation.Deepen, 0f);
            Assert.That(deep.Success, Is.True, deep.Reason);
            Assert.That(deep.Card.Threat, Is.EqualTo(ghost.Threat + 2));
            Assert.That(deep.Card.ValueOf(OmenEffectKind.LootPercent), Is.EqualTo(30f * 1.25f).Within(1e-4f));
            Assert.That(deep.Card.Effects.Count, Is.EqualTo(ghost.Effects.Count + 1));
            Assert.That(Table.Mutate(deep.Card, CardMutation.Deepen, 0f).Success, Is.False);
            Assert.That(CartomancerDefinition.CatalystFor(CardMutation.Deepen), Is.EqualTo(CatalystKind.Mastercraft));
        }

        [Test]
        public void Reroll_SwapsTheHandicap_KeepsTheBenefit()
        {
            var hive = Card(_r.HungryHive);
            var pool = Table.PoolFor(OmenFamily.Hive);
            var rerolled = Table.Mutate(hive, CardMutation.Reroll, 0.99f).Card;
            Assert.That(rerolled.Effects.Where(e => e.side == OmenSide.Handicap).Select(e => e.kind), Is.EqualTo(new[] { pool[pool.Count - 1].kind }));
            Assert.That(rerolled.ValueOf(OmenEffectKind.RarityRolls), Is.EqualTo(hive.ValueOf(OmenEffectKind.RarityRolls)));
        }

        // ---- Corruption -------------------------------------------------------------------------

        [TestCase(0.10f, CardCorruption.Nothing)]
        [TestCase(0.30f, CardCorruption.Ascended)]
        [TestCase(0.60f, CardCorruption.Doubled)]
        [TestCase(0.90f, CardCorruption.Heretic)]
        public void Corruption_FourEqualOutcomes(float roll, CardCorruption expected)
        {
            Assert.That(Table.CorruptionOutcome(roll), Is.EqualTo(expected));
        }

        [Test]
        public void Corruption_IsIrreversible()
        {
            var corrupted = Table.Mutate(Card(Cs.ThickHide), CardMutation.Corrupt, 0.1f).Card;
            Assert.That(corrupted.IsCorrupted, Is.True);
            Assert.That(Table.AddDuplicate(corrupted, Cs.ThickHide, Rarity.Common).Success, Is.False);
            Assert.That(Table.LevelUp(corrupted, 10_000, false).Success, Is.False);
            Assert.That(Table.Ascend(corrupted, 10_000, Rarity.Legendary).Success, Is.False);
            foreach (CardMutation mutation in Enum.GetValues(typeof(CardMutation)))
            {
                Assert.That(Table.Mutate(corrupted, mutation, 0.5f).Success, Is.False, mutation.ToString());
            }
        }

        [Test]
        public void Corruption_Heretic_TurnsThinSwarmIntoItsHereticVariant()
        {
            var heretic = Table.Mutate(Card(Cs.ThinSwarm), CardMutation.Corrupt, 0.9f).Card;
            Assert.That(heretic.Definition, Is.SameAs(Cs.ThinSwarmHeretic));
            Assert.That(heretic.ValueOf(OmenEffectKind.ChampionMultiplier), Is.EqualTo(15f), "every alien a champion");
            Assert.That(Cs.ThinSwarmHeretic.IsHeretic, Is.True);
        }

        [Test]
        public void Corruption_HereticWithoutAVariant_FallsBackToDoubled_WhichDoublesThePayout()
        {
            var blood = Card(_r.BloodPrice);
            var doubled = Table.Mutate(blood, CardMutation.Corrupt, 0.9f).Card;
            Assert.That(doubled.Corruption, Is.EqualTo(CardCorruption.Doubled));
            Assert.That(doubled.ValueOf(OmenEffectKind.AlienHealthPercent), Is.EqualTo(75f).Within(1e-4f));
            Assert.That(doubled.BrutalityPayoutPercent, Is.EqualTo(blood.BrutalityPayoutPercent * 2f).Within(1e-4f));
        }

        [Test]
        public void Corruption_Ascended_AddsARarity_OrATwistAtLegendary()
        {
            var up = Table.Mutate(Card(Cs.ThickHide), CardMutation.Corrupt, 0.3f).Card;
            Assert.That(up.Rarity, Is.EqualTo(Rarity.Uncommon));
            Assert.That(up.Level, Is.EqualTo(1));
        }

        // ---- The schedule -----------------------------------------------------------------------

        [Test]
        public void Schedule_Bench4_Cartomancer6_Mutations7_Corruption9()
        {
            int First(Func<MissionOmenSchedule, bool> open) => Rollout.Schedule.First(open).mission;
            Assert.That(First(s => s.benchOpen), Is.EqualTo(4));
            Assert.That(First(s => s.cartomancerOpen), Is.EqualTo(6));
            Assert.That(First(s => s.mutationsOpen), Is.EqualTo(7));
            Assert.That(First(s => s.corruptionOpen), Is.EqualTo(9));
        }

        // ---- Catalysts and the bench ------------------------------------------------------------

        [Test]
        public void Catalysts_TwentyTwoKinds_EachUsableSomewhere()
        {
            Assert.That(_r.Catalysts.All, Has.Count.EqualTo(22));
            Assert.That(_r.Catalysts.All.Select(c => c.Kind).Distinct().Count(), Is.EqualTo(22));
            Assert.That(_r.Catalysts.All, Has.All.Matches<CatalystDefinition>(c => c.Uses != CatalystUse.None));
            Assert.That(_r.Catalysts.All.Count(c => c.IsMeta), Is.EqualTo(8));
        }

        [Test]
        public void StripTemperSeal_AreBenchOnly_AndDropFromMissionFour()
        {
            foreach (var kind in new[] { CatalystKind.Strip, CatalystKind.Temper, CatalystKind.Seal })
            {
                var catalyst = _r.Catalysts[kind];
                Assert.That(catalyst.Uses, Is.EqualTo(CatalystUse.Bench), kind.ToString());
                Assert.That(catalyst.DropsFromMission, Is.EqualTo(4), kind.ToString());
            }

            Assert.That(_r.Catalysts[CatalystKind.Focus].CanUse(CatalystUse.Bench), Is.False, "Focus shapes a drop, not an owned item");
        }

        [Test]
        public void EveryCardMutation_UsesACatalystThatWorksOnCards()
        {
            foreach (CardMutation mutation in Enum.GetValues(typeof(CardMutation)))
            {
                Assert.That(_r.Catalysts[CartomancerDefinition.CatalystFor(mutation)].CanUse(CatalystUse.Card), Is.True, mutation.ToString());
            }
        }

        [TestCase(0.10f, InfusionOutcome.Nothing)]
        [TestCase(0.30f, InfusionOutcome.Overgrowth)]
        [TestCase(0.60f, InfusionOutcome.Xenomorphic)]
        [TestCase(0.90f, InfusionOutcome.Rot)]
        public void Infusion_FourEqualOutcomes(float roll, InfusionOutcome expected)
        {
            Assert.That(InfusionMath.OutcomeOf(roll), Is.EqualTo(expected));
        }

        [Test]
        public void Infusion_Overgrowth_GoesOnePastTheLinesMaximum()
        {
            Assert.That(InfusionMath.Infuse(Rarity.Common, Mark.MkVIII, Mark.MkVIII, 0.3f).NewMark, Is.EqualTo(Mark.MkIX));
            Assert.That(InfusionMath.Infuse(Rarity.Common, Mark.MkIII, Mark.MkVIII, 0.3f).NewMark, Is.EqualTo(Mark.MkIV));
            Assert.That(InfusionMath.Infuse(Rarity.Common, Mark.MkXII, Mark.MkXII, 0.3f).NewMark, Is.EqualTo(Mark.MkXII), "nothing past Mk XII");
            Assert.That(InfusionMath.Infuse(Rarity.Common, Mark.MkIII, Mark.MkVIII, 0.1f).NewMark, Is.Null);
        }

        [Test]
        public void Infusion_RarityShrinksTheRisk()
        {
            Assert.That(InfusionMath.Infuse(Rarity.Uncommon, Mark.MkI, Mark.MkVIII, 0.9f).SocketsProtected, Is.False);
            Assert.That(InfusionMath.Infuse(Rarity.Rare, Mark.MkI, Mark.MkVIII, 0.9f).SocketsProtected, Is.True);
            Assert.That(InfusionMath.Infuse(Rarity.Legendary, Mark.MkI, Mark.MkVIII, 0.9f, 0.3f).Outcome, Is.EqualTo(InfusionOutcome.Overgrowth),
                "a Legendary keeps the better of two rolls");
        }

        // ---- The Anchorite ----------------------------------------------------------------------

        [Test]
        public void TheAnchorite_IsAMajor_WithAThreatFloor_AndNoCommons()
        {
            var a = Card(_r.TheAnchorite);
            Assert.That(a.Arcana, Is.EqualTo(OmenArcana.Major));
            Assert.That(a.ValueOf(OmenEffectKind.ThreatFloor), Is.EqualTo(12f));
            Assert.That(a.ValueOf(OmenEffectKind.NoCommonDrops), Is.EqualTo(1f));
            Assert.That(a.ValueOf(OmenEffectKind.RareDropChancePercent), Is.EqualTo(50f));
            Assert.That(Rollout.Entries.Select(e => e.card), Has.No.Member(_r.TheAnchorite), "Act 3, not missions 1–10");
        }

        // ---- Thread safety ----------------------------------------------------------------------

        [Test]
        public void Instances_AreSafeToReadInParallel()
        {
            var deck = new List<OmenCardInstance> { LevelFive(Cs.ThickHide), Card(_r.TheTithe), Card(Cs.GleanersMoon) };
            var expected = OmenLoadoutRules.Stacked(deck, OmenEffectKind.AlienHealthPercent);
            var results = new float[64];
            System.Threading.Tasks.Parallel.For(0, results.Length, i => results[i] = OmenLoadoutRules.Stacked(deck, OmenEffectKind.AlienHealthPercent));
            Assert.That(results, Has.All.EqualTo(expected));
        }
    }
}
