using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>The thirteen fixes from the "Review: missions 1–3" tab, checked against the data.</summary>
    public sealed class ReviewFixesTests
    {
        private InMemoryDefinitionFactory _factory;
        private TremolinoContent _t;
        private ColdSleepContent _c;
        private PrologueContent _p;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _t = TremolinoContentBuilder.Build(_factory);
            _c = _t.Previous;
            _p = _c.Prologue;
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void AllCatalogs_HaveNoValidationErrors()
        {
            var reports = new[] { _p.Catalog.Validate(), _c.Catalog.Validate(), _t.Catalog.Validate() };
            var errors = reports.SelectMany(r => r.Issues).Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        // 1. The goals panel: every mission names the next long-term goal.
        [Test]
        public void Fix1_EveryMissionEnds_WithANamedNextGoal()
        {
            Assert.That(_p.Mission.NextGoal, Is.SameAs(_p.NewNameGoal));
            Assert.That(_c.Trip2.NextGoal, Is.SameAs(_c.ServoArmsGoal));
            Assert.That(_t.Mission.NextGoal, Is.SameAs(_t.RescueGoal));
        }

        [Test]
        public void Fix1_GoalProgress_TracksTheCampaign()
        {
            var state = new CampaignState(_t.Factions.NewStanding());
            var goal = _c.ServoArmsGoal;

            Assert.That(goal.StepsDone(state), Is.Zero);
            state.Add(_c.SyntheticMyomer, 1);
            state.Add(_p.GravLatticeAlloy, 1);
            state.Add(_c.XenoalloyIngot, 2);
            Assert.That(goal.StepsDone(state), Is.EqualTo(2));
            Assert.That(goal.Steps.Single(s => s.item == _c.XenoalloyIngot).Progress(state), Is.EqualTo(0.5f));

            _t.Mission.Complete(state.Standing);
            Assert.That(goal.Steps.Single(s => s.kind == GoalStepKind.HaveFlag).IsDone(state), Is.True, "the Tremolino grants T2 access");
        }

        // 2. The mission 3 climax can't fail on mission 2 choices.
        [Test]
        public void Fix2_TheTremolinoAloneSuppliesEverythingTheMk2Needs()
        {
            var recipe = _c.ServoArmsMk2Recipe;
            var chunksNeeded = recipe.AmountOf(_c.XenoalloyIngot) * _c.XenoalloyRecipe.AmountOf(_c.AmalgamChunk);
            var circuitry = _t.PartsStore.AmountOf(_p.Circuitry) + _t.SecuritySynthRemains.AmountOf(_p.Circuitry);

            Assert.That(_t.PartsStore.AmountOf(_c.AmalgamChunk), Is.GreaterThanOrEqualTo(chunksNeeded));
            Assert.That(_t.SecuritySynthRemains.AmountOf(_c.SyntheticMyomer), Is.GreaterThanOrEqualTo(recipe.AmountOf(_c.SyntheticMyomer)));
            Assert.That(circuitry, Is.GreaterThanOrEqualTo(recipe.AmountOf(_p.Circuitry)));
            Assert.That(_t.DeepHold.Items.Any(i => i.item == _p.GravLatticeAlloy), Is.True);
        }

        // 3. The commander's cards: one or two new per mission.
        [Test]
        public void Fix3_TheCommanderHasCardsFromThePrologue()
        {
            var hand = _p.Mission.CardsGranted.Concat(_c.Trip1.CardsGranted).Concat(_t.Mission.CardsGranted).ToList();

            Assert.That(_p.Mission.CardsGranted.Single().Effect, Is.EqualTo(CountermeasureEffect.VentRoom));
            Assert.That(_p.Mission.Beats.Single(b => b.Order == 26).PromptBody, Does.Contain("Vent"));
            Assert.That(hand.Select(c => c.Effect), Is.EquivalentTo(new[]
            {
                CountermeasureEffect.VentRoom, CountermeasureEffect.SealDoor, CountermeasureEffect.CutLights, CountermeasureEffect.TurnHulk
            }));
        }

        // 4. The prologue teaches with 12 cards, not 27.
        [Test]
        public void Fix4_ThePrologueHasTwelvePromptCards_AndTwoKindsOfMasking()
        {
            Assert.That(_p.Mission.CountPrompts(PromptStyle.Card), Is.EqualTo(12));
            Assert.That(_p.Mission.CountPrompts(PromptStyle.Toast), Is.EqualTo(15));
            Assert.That(_p.All.OfType<RoomAmbienceDefinition>().Select(a => a.Pattern), Is.EquivalentTo(new[] { AmbiencePattern.Steady, AmbiencePattern.Burst }));
            Assert.That(_c.BallastPumps.Pattern, Is.EqualTo(AmbiencePattern.Pulsing), "pulsing masking moved to mission 2");
            Assert.That(_c.BallastPumps.IsMasked(_p.HandCart.NoiseMoving, 0.5f), Is.True);
            Assert.That(_c.BallastPumps.IsMasked(_p.HandCart.NoiseMoving, 2.5f), Is.False);
        }

        // 5. Rewards every few minutes.
        [Test]
        public void Fix5_FindsAreMarkedOnTheMap_AndPodsPayWhenTheyComeFree()
        {
            Assert.That(_p.StoresCrates, Has.Count.EqualTo(3));
            Assert.That(_p.StoresCrates.All(c => c.MarkedOnMap), Is.True);
            Assert.That(_p.ShipsStores.ScripValue, Is.EqualTo(100));
            Assert.That(_p.Mission.LoreLogs, Has.Count.GreaterThanOrEqualTo(4));
            Assert.That(_c.PodMount.SalvageBonusScrip, Is.EqualTo(100));
            Assert.That(_c.CrewStash.OmenCards.Single().Rarity, Is.EqualTo(Rarity.Uncommon));
            Assert.That(_t.Mission.LoreLogs, Has.Count.GreaterThanOrEqualTo(3));
        }

        // 6. Commander rank: 2 after the prologue, 4 by the end of mission 2 (with or without the Judea), 5 in mission 3.
        [TestCase(false)]
        [TestCase(true)]
        public void Fix6_CommanderRanks_FollowTheTargets(bool didJudea)
        {
            var ranks = _p.Ranks;
            var xp = _p.Mission.CommanderXp;
            Assert.That(ranks.RankAt(xp), Is.EqualTo(2), "after the prologue");

            xp += _c.Trip1.CommanderXp + (didJudea ? _c.Judea.CommanderXp : 0) + _c.Trip2.CommanderXp;
            Assert.That(ranks.RankAt(xp), Is.EqualTo(4), $"end of mission 2 ({xp} XP)");

            xp += _t.Mission.CommanderXp;
            Assert.That(ranks.RankAt(xp), Is.EqualTo(5), $"after mission 3 ({xp} XP)");
            Assert.That(ranks.MaxRank, Is.EqualTo(20));
        }

        // 7. Charge and standing orders are taught on purpose.
        [Test]
        public void Fix7_ChargeAndStandingOrders_AreTaughtInMission2()
        {
            var trip1 = _c.Trip1.Beats.SelectMany(b => b.Objectives).Select(o => o.objectiveId).ToList();
            var trip2 = _c.Trip2.Beats.SelectMany(b => b.Objectives).Select(o => o.objectiveId).ToList();

            Assert.That(_p.Mission.ChargeEnabled, Is.False, "no charge in the prologue");
            Assert.That(_c.Trip1.ChargeEnabled, Is.True);
            Assert.That(trip1, Has.Member("obj_recharge").And.Member("obj_staff_printer"));
            Assert.That(trip2, Has.Member("obj_supply_order"));
            Assert.That(_p.J1M.UsesCharge, Is.True);
        }

        // 8. The specialization choice is informed.
        [Test]
        public void Fix8_TheBoardSaysWhatTheJudeaPays_WhenPicketChooses()
        {
            Assert.That(_c.JudeaContract.RewardSummary, Does.Contain("conversion kit"));
            Assert.That(_c.Trip1.Beats.Last().PromptBody, Does.Contain("Judea"));
        }

        // 9. Patusan opens in stages.
        [Test]
        public void Fix9_PatusanOpensInStages()
        {
            var offers = _c.All.OfType<MarketOfferDefinition>().ToList();
            var standing = _t.Factions.NewStanding();

            var first = offers.Where(o => o.IsVisibleFor(standing)).Select(o => o.Id).ToList();
            Assert.That(first, Is.EquivalentTo(new[] { "offer_new_identity", "offer_transponder_reflash", "offer_repair_j1m", "offer_picket" }));

            _c.Trip1.Complete(standing);
            var second = offers.Count(o => o.IsVisibleFor(standing));
            Assert.That(second, Is.EqualTo(4 + 1 + 8), "the android dealer and the clinic open after trip 1");

            _c.Contract.Complete(standing);
            Assert.That(offers.Count(o => o.IsVisibleFor(standing)), Is.GreaterThan(second + 40), "the teaser shelf and the mines open after the contract");
        }

        // 10. Trip 2 has safety nets.
        [Test]
        public void Fix10_Trip2_CheckpointsAfterTheNest_AndOffersMercy()
        {
            Assert.That(_c.Trip2.Beats.Single(b => b.Id == "beat_cs2_03_sleeper").IsCheckpoint, Is.True);
            Assert.That(_c.Trip2.MercyAfterFailures, Is.EqualTo(2));
            Assert.That(_c.Trip2.MercyOmen, Is.SameAs(_c.ThinSwarm));
        }

        // 11. Every turn of the Tremolino autosaves.
        [Test]
        public void Fix11_EveryTurnAutosaves()
        {
            Assert.That(_t.Mission.AutosaveOnEvents, Has.Count.EqualTo(_t.Sun.Facings));
        }

        // 12. J1M's firmware has its moment, and the sleeper is next.
        [Test]
        public void Fix12_J1MGlitchesAtTheNest_AndTheSleeperIsNext()
        {
            Assert.That(_p.J1M.StatsAt(Mark.MkI).Firmware, Is.EqualTo(FirmwareGeneration.Gen1));
            Assert.That(_p.J1M.MarkCurve.GlitchChance(Mark.MkI, incidentInvolvesSpecimen: true), Is.GreaterThan(0f));
            Assert.That(_t.Mission.Beats.Any(b => b.Id == "beat_tr_06_directive"), Is.True);
            Assert.That(_t.RescueGoal.Steps.First().flag, Is.EqualTo("torrens_sleeper_logged"));
            Assert.That(_c.Trip2.CompletionFlags, Has.Member("torrens_sleeper_logged"));
        }

        // 13. The stall.
        [Test]
        public void Fix13_TheStall_PaysBetterThanTheExchange_UntilTheLedgerGoesToTheLane()
        {
            var stall = _c.Stall;
            var standing = _t.Factions.NewStanding();
            _c.Contract.Complete(standing);

            const int exchangeValue = 60;
            var net = stall.NetSalePrice(exchangeValue, 0, inDemand: false, contraband: false, standing.Of(_t.Factions.HollowMoon));
            var demand = stall.NetSalePrice(exchangeValue, 0, inDemand: true, contraband: false, standing.Of(_t.Factions.HollowMoon));

            Assert.That(standing.HasFlag(stall.UnlockFlag), Is.True, "Doramin offers it when Cold Sleep completes");
            Assert.That(net, Is.GreaterThan(exchangeValue * 1.4f), $"{net:0} at the stall vs {exchangeValue} at the Exchange");
            Assert.That(demand, Is.EqualTo(net * 1.5f).Within(0.01f));
            Assert.That(stall.IsSeized(standing), Is.False);

            _t.Ledger.Choose("lane", standing);
            Assert.That(stall.IsSeized(standing), Is.True, "the Hollow Moon takes the stall back");
        }
    }
}
