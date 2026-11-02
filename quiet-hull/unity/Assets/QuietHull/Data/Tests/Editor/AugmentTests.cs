using System;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>The augment and worker-module catalog, mark scaling, loadouts, worker marks and charge.</summary>
    public sealed class AugmentTests
    {
        private InMemoryDefinitionFactory _factory;
        private ColdSleepContent _c;
        private AugmentCatalog _a;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _c = ColdSleepContentBuilder.Build(_factory);
            _a = _c.Augs;
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void Catalog_HasNoValidationErrors()
        {
            var errors = _a.Catalog.Validate().Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void Catalog_FourLinesPerBodySlot_AndAtLeastFourModulesPerWorkSlot()
        {
            Assert.That(_a.SynthAugments, Has.Count.EqualTo(32));
            for (var slot = AugmentSlot.Arms; slot <= AugmentSlot.Brain; slot++)
            {
                Assert.That(_a.SynthAugments.Count(x => x.Slot == slot), Is.EqualTo(4), slot.ToString());
            }

            for (var slot = AugmentSlot.Frame; slot <= AugmentSlot.ToolMount; slot++)
            {
                Assert.That(_a.WorkerModules.Count(x => x.Slot == slot), Is.GreaterThanOrEqualTo(4), slot.ToString());
            }

            Assert.That(_a.HardeningModules, Has.Count.EqualTo(6), "no-G, radiation, acid, heat, cold, EMP");
        }

        // ---- Mark scaling ---------------------------------------------------------------------

        [TestCase(Mark.MkI, 1)]
        [TestCase(Mark.MkII, 1)]
        [TestCase(Mark.MkIII, 2)]
        [TestCase(Mark.MkIV, 2)]
        [TestCase(Mark.MkV, 3)]
        [TestCase(Mark.MkVI, 3)]
        [TestCase(Mark.MkVII, 4)]
        [TestCase(Mark.MkVIII, 5)]
        [TestCase(Mark.MkXII, 5)]
        public void FabricatorTier_ByMark(Mark mark, int tier) => Assert.That(AugmentMath.RequiredFabricatorTier(mark), Is.EqualTo(tier));

        [Test]
        public void MarkVIII_IsAboutThreeTimesTheEffect_AtFortyTimesThePrice()
        {
            var arms = _a.Synth(AugmentSlot.Arms, AugmentLine.Standard);

            Assert.That(AugmentMath.EffectFactor(Mark.MkVIII), Is.EqualTo(3.1f).Within(1e-4f));
            Assert.That(arms.PriceAt(Mark.MkVIII) / (float)arms.PriceAt(Mark.MkI), Is.InRange(40f, 42f));
            Assert.That(arms.HasMark(Mark.MkVIII), Is.True);
            Assert.That(arms.HasMark(Mark.MkIX), Is.False, "synth parts stop at Mk VIII");
            Assert.That(_a.LoaderFrame.HasMark(Mark.MkXII), Is.True, "worker modules go to Mk XII");
        }

        [Test]
        public void AssaultParts_GetLouderAsTheyGetStronger_ShadowPartsGetQuieter()
        {
            var plate = _a.Synth(AugmentSlot.Dermis, AugmentLine.Assault);
            var gait = _a.Synth(AugmentSlot.Legs, AugmentLine.Shadow);

            Assert.That(plate.NoiseAt(Mark.MkV), Is.GreaterThan(plate.NoiseAt(Mark.MkI)));
            Assert.That(plate.WeightAt(Mark.MkV), Is.GreaterThan(plate.WeightAt(Mark.MkI)));
            Assert.That(gait.NoiseAt(Mark.MkV), Is.LessThan(gait.NoiseAt(Mark.MkI)));
            Assert.That(gait.WeightAt(Mark.MkV), Is.LessThan(gait.WeightAt(Mark.MkI)));
        }

        [TestCase(Mark.MkI, 1f)]
        [TestCase(Mark.MkIV, 1f)]
        [TestCase(Mark.MkV, 2f)]
        [TestCase(Mark.MkIX, 3f)]
        [TestCase(Mark.MkXII, 3f)]
        public void Hardening_StepsUpEveryFourMarks_CappedAtThree(Mark mark, float rating)
        {
            var maglock = _a.HardeningModules.Single(m => m.Id == "wmod_hard_maglock");
            Assert.That(StatMath.SumFlat(UnitStat.GravityShielding, maglock.ModifiersAt(mark)), Is.EqualTo(rating));
        }

        // ---- Loadouts: assault vs shadow -------------------------------------------------------

        private LoadoutProfile Picket(AugmentLine line, Mark mark) => LoadoutProfile.Compute(_c.Picket, Mark.MkI, _a.FullBody(line, mark));

        [Test]
        public void FullAssault_IsLoudHotAndDrawsAliens()
        {
            var assault = Picket(AugmentLine.Assault, Mark.MkI);
            var bare = LoadoutProfile.Compute(_c.Picket, Mark.MkI, Array.Empty<InstalledAugment>());

            Assert.That(assault.IsValid, Is.True, string.Join("; ", assault.Problems));
            Assert.That(assault.NoiseWalking, Is.GreaterThan(bare.NoiseWalking + 4f), $"noise {assault.NoiseWalking:0.#}");
            Assert.That(assault.HeatIdle, Is.GreaterThan(bare.HeatIdle + 5f), $"heat {assault.HeatIdle:0.#}");
            Assert.That(assault.AggroPercent, Is.GreaterThanOrEqualTo(0.5f), $"aggro {assault.AggroPercent:P0}");
            Assert.That(assault.Stat(UnitStat.Armour, _c.Picket.Armour), Is.GreaterThan(45f));
        }

        [Test]
        public void FullShadow_IsNearlySilent_HardToSee_AndContraband()
        {
            var shadow = Picket(AugmentLine.Shadow, Mark.MkI);

            Assert.That(shadow.IsValid, Is.True, string.Join("; ", shadow.Problems));
            Assert.That(shadow.NoiseWalking, Is.LessThanOrEqualTo(0.5f), $"noise {shadow.NoiseWalking:0.#}");
            Assert.That(shadow.HeatIdle, Is.LessThanOrEqualTo(1f), $"heat {shadow.HeatIdle:0.#}");
            Assert.That(shadow.VisibilityPercent, Is.LessThanOrEqualTo(-0.5f), $"visibility {shadow.VisibilityPercent:P0}");
            Assert.That(shadow.AggroPercent, Is.LessThan(0f));
            Assert.That(shadow.ContrabandCount, Is.EqualTo(8));
        }

        [Test]
        public void Shadow_CostsAtLeastFourTimesStandard()
        {
            Assert.That(Picket(AugmentLine.Shadow, Mark.MkI).TotalPriceScrip,
                Is.GreaterThanOrEqualTo(4 * Picket(AugmentLine.Standard, Mark.MkI).TotalPriceScrip));
        }

        [Test]
        public void HeavyLoadouts_GoOverweight_AndSlowDown()
        {
            var assault = Picket(AugmentLine.Assault, Mark.MkVIII);
            var unpenalized = assault.Stat(UnitStat.MoveSpeed, _c.Picket.MoveSpeed);

            Assert.That(assault.OverweightKg, Is.GreaterThan(0f), $"{assault.AugmentWeightKg:0} kg on a {_c.Picket.AugmentCapacityKg:0} kg frame");
            Assert.That(assault.MoveSpeed, Is.LessThan(unpenalized), "overweight costs speed");
            Assert.That(assault.NoiseWalking, Is.GreaterThan(Picket(AugmentLine.Assault, Mark.MkI).NoiseWalking), "and adds noise");
        }

        [Test]
        public void PowerOverdraw_HeatsTheUnit()
        {
            var noCore = _a.FullBody(AugmentLine.Assault, Mark.MkIII).Where(i => i.Definition.Slot != AugmentSlot.Heart).ToList();
            var profile = LoadoutProfile.Compute(_c.Picket, Mark.MkI, noCore);

            Assert.That(profile.OverdrawWatts, Is.GreaterThan(0f), "assault parts without a reactor draw more than the frame supplies");
            Assert.That(profile.CooldownPenaltyPercent, Is.GreaterThan(0f));
        }

        [Test]
        public void Loadout_RejectsWrongSlotsAndExtraParts()
        {
            var twoArms = new[]
            {
                new InstalledAugment(_a.Synth(AugmentSlot.Arms, AugmentLine.Standard), Mark.MkI),
                new InstalledAugment(_a.Synth(AugmentSlot.Arms, AugmentLine.Fine), Mark.MkI)
            };
            var workerOnSynth = new[] { new InstalledAugment(_a.LoaderFrame, Mark.MkI) };

            Assert.That(LoadoutProfile.Compute(_c.Picket, Mark.MkI, twoArms).IsValid, Is.False);
            Assert.That(LoadoutProfile.Compute(_c.Picket, Mark.MkI, workerOnSynth).IsValid, Is.False);
        }

        // ---- Workers ------------------------------------------------------------------------

        private LoadoutProfile Worker(Mark mark, params InstalledAugment[] modules) => LoadoutProfile.Compute(_c.SecondWorker, mark, modules);

        [Test]
        public void Workers_DefendOnly_MilitaryCanBeOrderedToAttack()
        {
            Assert.That(_c.SecondWorker.CanBeOrderedToAttack, Is.False);
            Assert.That(_c.SecondWorker.DefendRadius, Is.GreaterThan(0f), "they defend themselves and company assets nearby");
            Assert.That(_c.SecondWorker.Can(UnitAbility.SupplySentries), Is.True, "they can be told to keep sentries supplied");
            Assert.That(_c.Picket.CanBeOrderedToAttack, Is.True);
            Assert.That(_a.Shade.CanBeOrderedToAttack, Is.True);
        }

        [Test]
        public void WorkerMarks_GoToXII_AndGrowEveryStat()
        {
            var mk1 = _c.SecondWorker.StatsAt(Mark.MkI);
            var mk12 = _c.SecondWorker.StatsAt(Mark.MkXII);

            Assert.That(_c.SecondWorker.MaxMark, Is.EqualTo(Mark.MkXII));
            Assert.That(mk12.MaxHealth, Is.EqualTo(mk1.MaxHealth * 2.65f).Within(0.5f));
            Assert.That(mk12.Armour, Is.EqualTo(mk1.Armour + 33f).Within(0.01f));
            Assert.That(mk12.HandCarrySlots, Is.EqualTo(mk1.HandCarrySlots + 2));
            Assert.That(mk12.NoiseWalking, Is.LessThan(mk1.NoiseWalking));
            Assert.That(mk12.PriceScrip, Is.GreaterThan(20 * mk1.PriceScrip));
        }

        [TestCase(Mark.MkI, FirmwareGeneration.Gen1, 0.25f, 0.25f)]
        [TestCase(Mark.MkV, FirmwareGeneration.Gen2, 0f, 0.10f)]
        [TestCase(Mark.MkIX, FirmwareGeneration.Gen3, 0f, 0f)]
        public void OldFirmware_Glitches_AndProtectsEggsAndTheQueen(Mark mark, FirmwareGeneration gen, float propertyChance, float specimenChance)
        {
            var curve = _c.SecondWorker.MarkCurve;

            Assert.That(curve.FirmwareAt(mark), Is.EqualTo(gen));
            Assert.That(curve.GlitchChance(mark, incidentInvolvesSpecimen: false), Is.EqualTo(propertyChance));
            Assert.That(curve.GlitchChance(mark, incidentInvolvesSpecimen: true), Is.EqualTo(specimenChance));
        }

        [Test]
        public void HeavyHauler_CarriesACryoBedAlone_ButEveryAlienKnows()
        {
            var loader = Worker(Mark.MkV, new InstalledAugment(_a.LoaderFrame, Mark.MkV));
            var hauler = StatMath.SumFlat(UnitStat.OversizeCarry, _a.Hauler.Passives);

            Assert.That(_a.Hauler.IsAvailable(new ItemDefinition[] { _a.LoaderFrame }), Is.True);
            Assert.That(_a.Hauler.IsAvailable(Array.Empty<ItemDefinition>()), Is.False, "needs a loader frame");
            Assert.That(hauler, Is.GreaterThanOrEqualTo(1f), "carries an oversize load like a cryo bed");
            Assert.That(loader.Stat(UnitStat.CarrySlots, 0f), Is.GreaterThanOrEqualTo(_c.CryoPod.Slots / 2f));
            Assert.That(loader.NoiseWalking, Is.GreaterThan(5f), $"noise {loader.NoiseWalking:0.#}");
            Assert.That(loader.AggroPercent + StatMath.SumFlat(UnitStat.Aggro, _a.Hauler.Passives), Is.GreaterThanOrEqualTo(0.7f));
        }

        [Test]
        public void StealthyWorker_ScoutsAndPlantsQuietly()
        {
            var quiet = Worker(Mark.MkI,
                new InstalledAugment(_a.MuffledFrame, Mark.MkI),
                new InstalledAugment(_a.SilentTools, Mark.MkI));
            var sapperNoise = StatMath.SumFlat(UnitStat.NoiseWalking, _a.Sapper.Passives);

            Assert.That(quiet.NoiseWalking + sapperNoise, Is.LessThanOrEqualTo(0f));
            Assert.That(quiet.VisibilityPercent, Is.LessThan(0f));
            Assert.That(StatMath.Apply(UnitStat.ActionNoise, 1f, quiet.Modifiers), Is.LessThan(0.5f), "installing a sentry is near silent");
        }

        [Test]
        public void Miner_GetsTheBeamHand_ConstructorSpendsLessPower()
        {
            Assert.That(_a.Miner.WeaponOverride, Is.SameAs(_a.MiningBeam));
            Assert.That(_a.MiningBeam.Slot, Is.EqualTo(AugmentSlot.ToolMount));

            var charge = AugmentCatalog.WorkerCharge;
            var plain = charge.SecondsOf(WorkActivity.Welding, Array.Empty<StatModifier>());
            var constructor = charge.SecondsOf(WorkActivity.Welding, _a.Constructor.Passives);
            Assert.That(constructor, Is.GreaterThan(plain * 1.3f), $"{constructor:0} s vs {plain:0} s of welding per charge");
        }

        [Test]
        public void Charge_HeavyWorkEmptiesItFast_StandingStillBarelyTouchesIt()
        {
            var charge = _c.SecondWorker.Charge;
            var none = Array.Empty<StatModifier>();

            Assert.That(_c.SecondWorker.UsesCharge, Is.True);
            Assert.That(charge.SecondsOf(WorkActivity.Walking, none), Is.EqualTo(1200f).Within(1f), "about 20 minutes walking");
            Assert.That(charge.SecondsOf(WorkActivity.Mining, none), Is.EqualTo(300f).Within(1f), "about 5 minutes mining");
            Assert.That(charge.SecondsOf(WorkActivity.CarryingOversize, none), Is.EqualTo(240f).Within(1f), "about 4 minutes carrying a cryo bed");
            Assert.That(charge.SecondsOf(WorkActivity.Idle, none), Is.GreaterThan(10000f));
            Assert.That(charge.SecondsToFull(none), Is.EqualTo(60f).Within(0.1f));
        }

        [Test]
        public void Workers_TakeFiveTrades()
        {
            Assert.That(_c.SecondWorker.SpecializationOptions.Select(s => s.Archetype),
                Is.EquivalentTo(new[] { Archetype.Hauler, Archetype.Miner, Archetype.Constructor, Archetype.Technician, Archetype.Sapper }));
        }
    
        // ---- The ammo line and charging ---------------------------------------------------------

        [Test]
        public void ChargingBay_MustBeUnweldedAndRepressurized_ThenBeatsTheShuttleCradle()
        {
            var bay = _c.TorrensChargingBay;

            Assert.That(bay.AccessRequires, Has.Member(ToolActionKind.CutBolt), "unweld it first");
            Assert.That(bay.RequiresAtmosphere, Is.True, "then restore the air");
            Assert.That(bay.AccessStepCount, Is.EqualTo(2));
            Assert.That(bay.ChargeBays, Is.GreaterThan(_c.NellieCradle.ChargeBays));
            Assert.That(bay.SecondsToCharge(_c.SecondWorker), Is.LessThan(_c.NellieCradle.SecondsToCharge(_c.SecondWorker)));
        }

        [Test]
        public void AmmoLine_WorkersTakeStandingOrders_AndEachRunsOutOfCharge()
        {
            var worker = _c.SecondWorker;
            var none = Array.Empty<StatModifier>();

            Assert.That(worker.Can(UnitAbility.StaffStation) && worker.Can(UnitAbility.Harvest) && worker.Can(UnitAbility.SupplySentries), Is.True);
            Assert.That(worker.Can(UnitAbility.AutoRecharge), Is.True);

            // The line breaks on its own if nobody recharges: the miner lasts 5 minutes, the fabricator operator under 7.
            Assert.That(worker.Charge.SecondsOf(WorkActivity.Mining, none), Is.LessThan(worker.Charge.SecondsOf(WorkActivity.Building, none)));
            Assert.That(worker.Charge.SecondsOf(WorkActivity.Building, none), Is.LessThan(worker.Charge.SecondsOf(WorkActivity.Walking, none)));
            // A runner that has to fight burns three times faster than one that walks.
            Assert.That(worker.Charge.Drain(WorkActivity.Defending), Is.EqualTo(3f * worker.Charge.Drain(WorkActivity.Walking)));
        }
    }
}
