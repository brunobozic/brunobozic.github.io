using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Mission 2, "Cold Sleep": Patusan (the hub) and the two-part cryo pod job on the Torrens.
    /// Values mirror the "Mission 2: Cold Sleep" tab of the design doc. Shared definitions
    /// (materials, sentries, Crawlers, Spitters, J1M) come from the prologue builder, so both
    /// catalogs reference the same assets.
    /// </summary>
    public static class ColdSleepContentBuilder
    {
        public const string CatalogId = "cat_cold_sleep";

        /// <summary>Spawn points on the Lane, the haul corridor both sentries cover.</summary>
        public const string LanePrefix = "lane_";

        // Economy from the doc.
        public const int AugmentPrice = 450;
        public const int PodPayPerTrip = 600;

        private const string UnitsFolder = "ColdSleep/Units";
        private const string ItemsFolder = "ColdSleep/Items";
        private const string GrowthFolder = "ColdSleep/Growth";
        private const string OmensFolder = "ColdSleep/Omens";
        private const string WorldFolder = "ColdSleep/World";
        private const string HubFolder = "ColdSleep/Hub";
        private const string ScriptFolder = "ColdSleep/Script";

        public static ColdSleepContent Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var prologue = PrologueContentBuilder.Build(factory);
            var make = new DefinitionMaker(factory);
            var c = new ColdSleepContent { Prologue = prologue };

            BuildWeapons(c, make);
            BuildGrowth(c, make);
            BuildUnits(c, make);
            BuildOmens(c, make);
            BuildWorld(c, make);
            BuildHub(c, make);
            BuildScripts(c, make);

            c.Catalog = make.WriteCatalog(CatalogId);
            c.All = make.All;
            return c;
        }

        private static void BuildWeapons(ColdSleepContent c, DefinitionMaker make)
        {
            var carbine = make.Def<RangedWeaponDefinition>("itm_picket_carbine", ItemsFolder, "Picket carbine",
                "Hollow Moon surplus. Reliable, unremarkable.", "weapon", "ranged");
            carbine.damagePerShot = 14f;
            carbine.fireIntervalSeconds = 0.2f;
            carbine.range = 20f;
            carbine.magazineSize = 30;
            carbine.reloadSeconds = 2f;
            carbine.noiseOnUse = 7f;
            carbine.heatOnUse = 3f;
            carbine.lightOnUse = LightEmission.Bright;
            c.PicketCarbine = carbine;

            var rifle = make.Def<RangedWeaponDefinition>("itm_long_rifle", ItemsFolder, "Long rifle",
                "A Marksman's rifle. Slow, accurate, devastating on a weak point.", "weapon", "ranged", "marksman");
            rifle.damagePerShot = 60f;
            rifle.fireIntervalSeconds = 1.5f;
            rifle.range = 28f;
            rifle.magazineSize = 5;
            rifle.reloadSeconds = 2.5f;
            rifle.noiseOnUse = 9f;
            rifle.heatOnUse = 3f;
            rifle.lightOnUse = LightEmission.Bright;
            c.LongRifle = rifle;

            var foam = make.Def<RangedWeaponDefinition>("itm_foam_projector", ItemsFolder, "Foam projector",
                "Sprays hardening foam. Hurts nothing; stops everything.", "weapon", "controller");
            foam.damagePerShot = 0f;
            foam.fireIntervalSeconds = 0.5f;
            foam.range = 14f;
            foam.magazineSize = 20;
            foam.reloadSeconds = 2f;
            foam.slowFraction = 0.4f;
            foam.slowSeconds = 3f;
            foam.noiseOnUse = 3f;
            foam.heatOnUse = 1f;
            foam.lightOnUse = LightEmission.None;
            c.FoamProjector = foam;
        }

        private static void BuildGrowth(ColdSleepContent c, DefinitionMaker make)
        {
            var marksman = make.Def<SpecializationDefinition>("spec_marksman", GrowthFolder, "Marksman",
                "Huge critical hits at long range.", "specialization");
            marksman.archetype = Archetype.Marksman;
            marksman.unlockExperience = 50;
            marksman.weaponOverride = c.LongRifle;
            marksman.passives = new[]
            {
                StatModifier.Add(UnitStat.CritChance, 0.30f),
                StatModifier.Add(UnitStat.CritMultiplier, 3f),
                StatModifier.Add(UnitStat.WeakPointMultiplier, 2f)
            };
            marksman.abilityId = "ab_overwatch";
            marksman.abilityName = "Overwatch";
            marksman.abilityDescription = "The first shot at anything entering a cone is a guaranteed critical.";
            marksman.abilityCooldownSeconds = 8f;
            marksman.abilityDurationSeconds = 0f;
            marksman.abilityRadius = 28f;
            marksman.strongAgainst = "Leapers: kills them during their 1 s crouch. Champions and Spitters at range.";
            marksman.weakAgainst = "Swarms: one slow shot at a time.";
            c.Marksman = marksman;

            var controller = make.Def<SpecializationDefinition>("spec_controller", GrowthFolder, "Controller",
                "Slows the swarm so the sentries can do the killing.", "specialization");
            controller.archetype = Archetype.Controller;
            controller.unlockExperience = 50;
            controller.weaponOverride = c.FoamProjector;
            controller.passives = new[] { StatModifier.Add(UnitStat.SentryHitRateVsSlowed, 0.25f) };
            controller.abilityId = "ab_foam_field";
            controller.abilityName = "Foam field";
            controller.abilityDescription = "A 5 m area for 6 s: aliens inside are slowed 40% and can't jump.";
            controller.abilityCooldownSeconds = 10f;
            controller.abilityDurationSeconds = 6f;
            controller.abilityRadius = 5f;
            controller.strongAgainst = "Swarms and Leapers, next to sentries.";
            controller.weakAgainst = "Anything the sentries can't reach: he can't kill on his own.";
            c.Controller = controller;

            var bulwark = make.Def<SpecializationDefinition>("spec_bulwark", GrowthFolder, "Bulwark",
                "Holds the line and regenerates faster than it takes damage.", "specialization");
            bulwark.archetype = Archetype.Bulwark;
            bulwark.unlockExperience = 50;
            bulwark.weaponOverride = null;
            bulwark.passives = new[]
            {
                StatModifier.Add(UnitStat.Armour, 30f),
                StatModifier.Add(UnitStat.HealthRegenPerSecond, 18f) // Out-heals 3 Crawlers (16.6/s after armour); 4+ wear him down.
            };
            bulwark.abilityId = "ab_hold_the_line";
            bulwark.abilityName = "Hold the line";
            bulwark.abilityDescription = "Aliens within 6 m target the Bulwark for 5 s. A melee hit knocks a landing Leaper down for 2 s.";
            bulwark.abilityCooldownSeconds = 12f;
            bulwark.abilityDurationSeconds = 5f;
            bulwark.abilityRadius = 6f;
            bulwark.strongAgainst = "Leapers and swarms that reach the sentries.";
            bulwark.weakAgainst = "Spitters at range, and packs of four or more.";
            c.Bulwark = bulwark;

            AugmentDefinition Aug(AugmentSlot slot, string name, string description, params StatModifier[] modifiers)
            {
                var augment = make.Def<AugmentDefinition>($"aug_{slot.ToString().ToLowerInvariant()}_mk1", GrowthFolder,
                    $"{name} Mk I", description, "augment", "cyber");
                augment.slot = slot;
                augment.kind = AugmentKind.Cyber;
                augment.mark = Mark.MkI;
                augment.priceScrip = AugmentPrice;
                augment.modifiers = modifiers;
                return augment;
            }

            c.Augments = new[]
            {
                Aug(AugmentSlot.Arms, "Servo arms", "Faster hands: more damage from any weapon.",
                    StatModifier.Percent(UnitStat.FireRate, 0.15f), StatModifier.Percent(UnitStat.ReloadTime, -0.25f), StatModifier.Percent(UnitStat.MeleeDamage, 0.20f)),
                Aug(AugmentSlot.Legs, "Damped legs", "Faster and quieter.",
                    StatModifier.Percent(UnitStat.MoveSpeed, 0.20f), StatModifier.Add(UnitStat.NoiseWalking, -1f)),
                Aug(AugmentSlot.Eyes, "Optic array", "Sees further and through smoke; finds weak points.",
                    StatModifier.Add(UnitStat.VisionRange, 4f), StatModifier.Add(UnitStat.CritChance, 0.05f), StatModifier.Add(UnitStat.SeesThroughSmoke, 1f)),
                Aug(AugmentSlot.Ears, "Audio sensors", "Hears through one more door; puts close contacts on the tracker.",
                    StatModifier.Add(UnitStat.DoorsHeardThrough, 1f), StatModifier.Add(UnitStat.ContactDetectionRange, 12f)),
                Aug(AugmentSlot.Lungs, "Heat exchanger", "Shrugs off heat and cold; cools down faster.",
                    StatModifier.Add(UnitStat.HeatShielding, 1f), StatModifier.Add(UnitStat.ColdShielding, 1f), StatModifier.Add(UnitStat.HeatAfterExertion, -2f)),
                Aug(AugmentSlot.Heart, "Micro-reactor", "Repairs itself out of combat; abilities come back sooner.",
                    StatModifier.Add(UnitStat.HealthRegenOutOfCombat, 2f), StatModifier.Percent(UnitStat.AbilityCooldown, -0.15f)),
                Aug(AugmentSlot.Dermis, "Ablative dermis", "Armoured and thermally damped.",
                    StatModifier.Add(UnitStat.Armour, 15f), StatModifier.Add(UnitStat.HeatSignature, -1f)),
                Aug(AugmentSlot.Brain, "Tactical cortex", "Learns faster, reacts faster, resists EMP and hacking.",
                    StatModifier.Percent(UnitStat.ExperienceGain, 0.20f), StatModifier.Percent(UnitStat.TargetAcquisitionTime, -0.30f), StatModifier.Add(UnitStat.EmpResistance, 1f))
            };
        }

        private static void BuildUnits(ColdSleepContent c, DefinitionMaker make)
        {
            var picket = make.Def<UnitDefinition>("unit_picket", UnitsFolder, "Picket",
                "A cheap Hollow Moon combat droid: a carbine, a chassis, no frills.", "unit", "combat", "synthetic");
            picket.maxHealth = 260f;
            picket.armour = 15f;
            picket.canBeDestroyed = true;
            picket.wreckedSeconds = 0f;
            picket.rebootHealthFraction = 0.3f;
            picket.moveSpeed = 3.2f;
            picket.noiseWalking = 2.5f;
            picket.heatIdle = 3f;
            picket.heatAfterExertion = 6f;
            picket.exertionCooldownSeconds = 6f;
            picket.lampRange = 10f;
            picket.handCarrySlots = 0;
            picket.abilities = UnitAbility.Move | UnitAbility.Interact | UnitAbility.Attack | UnitAbility.Patrol | UnitAbility.FireWeapon;
            picket.startingItems = new ItemDefinition[] { c.PicketCarbine };
            picket.augmentSlots = (AugmentSlot[])Enum.GetValues(typeof(AugmentSlot));
            picket.specializationOptions = new[] { c.Marksman, c.Controller, c.Bulwark };
            c.Picket = picket;

            var j1m = c.Prologue.J1M;
            var worker = make.Def<UnitDefinition>("unit_j_series_worker", UnitsFolder, "J-series worker",
                "A used Lifter-class android from Patusan. The player names it.", "unit", "worker", "android");
            worker.maxHealth = 220f; // Not J1M: no prologue resilience.
            worker.armour = j1m.Armour;
            worker.canBeDestroyed = true;
            worker.wreckedSeconds = 0f;
            worker.rebootHealthFraction = 0.3f;
            worker.moveSpeed = j1m.MoveSpeed;
            worker.noiseWalking = j1m.NoiseWalking;
            worker.heatIdle = j1m.HeatIdle;
            worker.heatAfterExertion = j1m.HeatAfterExertion;
            worker.exertionCooldownSeconds = j1m.ExertionCooldownSeconds;
            worker.lampRange = j1m.LampRange;
            worker.handCarrySlots = j1m.HandCarrySlots;
            worker.abilities = j1m.Abilities;
            worker.startingItems = Array.Empty<ItemDefinition>();
            c.SecondWorker = worker;

            var leaper = make.Def<AlienDefinition>("alien_leaper", UnitsFolder, "Leaper",
                "Crouches for a second, then jumps 6 m over barricades onto sentries and the units holding a line.", "alien");
            leaper.maxHealth = 120f;
            leaper.armour = 10f;
            leaper.moveSpeed = 4f;
            leaper.usesDucts = false;
            leaper.attack = new AttackProfile(AttackKind.Melee, 10f, 1f, 1.5f, 0f);
            leaper.targetPriority = new[] { TargetKind.Sentry, TargetKind.NearestUnit };
            leaper.senses = new SenseProfile(1f, 0.5f, 12f, 120f, 0f);
            leaper.signalRadius = 0f;
            leaper.signalCooldownSeconds = 0f;
            leaper.giveUpSeconds = 12f;
            leaper.defaultBehaviour = AlienBehaviour.Stream;
            leaper.surgeCost = 4;
            leaper.chitinBounty = 3;
            leaper.experience = 4;
            c.Leaper = leaper;

            var constable = make.Def<AlienDefinition>("enemy_constable", UnitsFolder, "Constable",
                "The Torrens's security synthetic. Revived, it opens the store, then calls it theft.", "enemy", "synthetic", "miniboss");
            constable.maxHealth = 600f;
            constable.armour = 30f;
            constable.moveSpeed = 2.8f;
            constable.usesDucts = false;
            constable.attack = new AttackProfile(AttackKind.Melee, 15f, 1.2f, 1.8f, 0f);
            constable.targetPriority = new[] { TargetKind.NearestUnit };
            constable.senses = new SenseProfile(1f, 0f, 20f, 140f, 0f);
            constable.signalRadius = 0f;
            constable.signalCooldownSeconds = 0f;
            constable.giveUpSeconds = 30f;
            constable.defaultBehaviour = AlienBehaviour.HoldPosition;
            constable.surgeCost = 0;
            constable.chitinBounty = 0;
            constable.experience = 20;
            c.Constable = constable;
        }

        private static void BuildOmens(ColdSleepContent c, DefinitionMaker make)
        {
            var thin = make.Def<OmenCardDefinition>("omen_thin_swarm", OmensFolder, "Thin Swarm",
                "Fewer aliens, smaller rewards.", "omen", "expedition", "starter");
            thin.family = OmenFamily.Expedition;
            thin.rarity = Rarity.Common;
            thin.commonThreat = -3;
            thin.effects = new[]
            {
                new OmenEffect(OmenEffectKind.SurgeSizePercent, OmenSide.Benefit, ScalingRule.Reduction, 0.30f),
                new OmenEffect(OmenEffectKind.LootPercent, OmenSide.Handicap, ScalingRule.Linear, -25f),
                new OmenEffect(OmenEffectKind.NoEpicRolls, OmenSide.Handicap, ScalingRule.None, 1f)
            };
            thin.legendaryTwist = "No champions spawn.";
            c.ThinSwarm = thin;

            var thick = make.Def<OmenCardDefinition>("omen_thick_hide", OmensFolder, "Thick Hide",
                "Tougher aliens; choose the reward: more loot or more pay.", "omen", "brutality", "starter");
            thick.family = OmenFamily.Brutality;
            thick.rarity = Rarity.Common;
            thick.commonThreat = 2;
            thick.effects = new[] { new OmenEffect(OmenEffectKind.AlienHealthPercent, OmenSide.Handicap, ScalingRule.Linear, 25f) };
            thick.brutalityPayout = true;
            thick.legendaryTwist = "Kills of full-health aliens drop chitin ×2.";
            c.ThickHide = thick;

            var ghost = make.Def<OmenCardDefinition>("omen_the_ghost", OmensFolder, "The Ghost",
                "Vow: no unit detected while the hive is Dormant. Keep it for a large bonus; break it and the hive knows.", "omen", "vow", "starter");
            ghost.family = OmenFamily.Vow;
            ghost.rarity = Rarity.Common;
            ghost.commonThreat = 0;
            ghost.vowCondition = VowCondition.UndetectedWhileDormant;
            ghost.effects = new[]
            {
                new OmenEffect(OmenEffectKind.LootPercent, OmenSide.Benefit, ScalingRule.Linear, 30f),
                new OmenEffect(OmenEffectKind.ContractPayPercent, OmenSide.Benefit, ScalingRule.Linear, 20f)
            };
            ghost.brokenEffects = new[] { new OmenEffect(OmenEffectKind.AwarenessJumpPercent, OmenSide.Handicap, ScalingRule.Linear, 20f) };
            ghost.legendaryTwist = "While the vow holds, footsteps make no noise.";
            c.TheGhost = ghost;
        }

        private static void BuildWorld(ColdSleepContent c, DefinitionMaker make)
        {
            var p = c.Prologue;

            var pod = make.Def<CargoDefinition>("itm_cryo_pod", ItemsFolder, "Cryo pod",
                "A colonial sleep pod. With one, a person can leave. That's why they're so scarce.", "cargo", "contract", "oversize");
            pod.slots = 6;
            pod.maxCondition = 100f;
            pod.conditionLossPerSecondByHeat = new[] { 0f, 0.5f, 1.5f, 3f };
            pod.explodesWhenDestroyed = false;
            pod.explosionDamage = 0f;
            pod.explosionRadius = 0f;
            c.CryoPod = pod;

            var cell = make.Def<CargoDefinition>("itm_shuttle_power_cell", ItemsFolder, "Shuttle power cell",
                "Pulled from the Nellie to wake the security synth.", "cargo", "mission");
            cell.slots = 1;
            cell.maxCondition = 100f;
            cell.conditionLossPerSecondByHeat = new[] { 0f, 0f, 0f, 0f };
            c.PowerCell = cell;

            var panel = make.Def<MaterialDefinition>("mat_acoustic_panel", ItemsFolder, "Acoustic panel",
                "Dense foam-and-lead sheet from the security store.", "material");
            panel.maxStack = 16;
            c.AcousticPanel = panel;

            var insulation = make.Def<RoomTreatmentDefinition>("rt_acoustic_insulation", WorldFolder, "Acoustic insulation",
                "Line a room and the noise of the work stays inside.", "treatment", "insulated");
            insulation.panel = panel;
            insulation.panelsRequired = 8;
            insulation.installSecondsPerPanel = 6f;
            insulation.soundMultiplier = 0.1f;
            insulation.vibrationMultiplier = 0.5f;
            insulation.containedThreshold = 1f;
            c.AcousticInsulation = insulation;

            var mount = make.Def<FixtureDefinition>("fx_cryo_pod_mount", WorldFolder, "Cryo pod mount",
                "Bolted down and wired into the ship. Disconnecting takes time and makes a racket.", "fixture");
            mount.yields = pod;
            mount.requiredAction = ToolActionKind.DetachFixture;
            mount.detachSeconds = 90f;
            mount.noise = 9f;
            mount.vibration = 6f;
            mount.interruptConditionLoss = 25f;
            mount.placedCount = 2;
            c.PodMount = mount;

            var store = make.Def<LootCacheDefinition>("cache_security_store", WorldFolder, "Security store",
                "Opened by Constable. Riot gear, restraints and the acoustic panels.", "cache", "locked");
            store.materials = new[] { new MaterialCost(panel, 8) };
            store.items = Array.Empty<ItemStack>();
            store.accessRequires = Array.Empty<ToolActionKind>();
            store.placedCount = 1;
            c.SecurityStore = store;

            var bulkhead = make.Def<LootCacheDefinition>("cache_bulkhead_panel", WorldFolder, "Bulkhead panel",
                "Ballast-deck plating. The only scrap on the Torrens; shared by both trips.", "cache", "salvage");
            bulkhead.materials = new[] { new MaterialCost(p.Scrap, 6) };
            bulkhead.items = Array.Empty<ItemStack>();
            bulkhead.accessRequires = new[] { ToolActionKind.CutPanel };
            bulkhead.placedCount = 10;
            c.BulkheadPanel = bulkhead;

            var ammo = make.Def<RecipeDefinition>("rcp_sentry_ammo_salvage", WorldFolder, "Sentry ammo box (salvage)",
                "The Torrens's fabricator prints rounds from scrap.", "recipe");
            ammo.output = p.SentryAmmoBox;
            ammo.outputCount = 1;
            ammo.inputs = new[] { new MaterialCost(p.Scrap, 6) };
            ammo.craftSeconds = 10f;
            ammo.requiresBlueprint = false;
            c.SalvageAmmoRecipe = ammo;

            var fabricator = make.Def<WorkstationDefinition>("ws_torrens_fabricator", WorldFolder, "Torrens fabricator (T1)",
                "Old, slow, still works.", "workstation", "fabricator");
            fabricator.services = WorkstationService.Fabricate;
            fabricator.recipes = new[] { ammo };
            fabricator.repairHealthPerSecond = 0f;
            fabricator.repairMaxSeconds = 0f;
            fabricator.requiresHack = false;
            fabricator.interactSeconds = 1f;
            c.TorrensFabricator = fabricator;
        }

        private static void BuildHub(ColdSleepContent c, DefinitionMaker make)
        {
            var cradle = make.Def<ShuttleModuleDefinition>("mod_cargo_cradle", HubFolder, "Cargo Cradle",
                "Carries one pod-sized load. Doramin's gift: he wants you carrying for him again.", "module");
            cradle.moduleSlotsUsed = 1;
            cradle.extraHoldSlots = 0;
            cradle.oversizeLoadCapacity = 1;
            cradle.priceScrip = 1500;
            c.CargoCradle = cradle;

            MarketOfferDefinition Offer(string id, string name, OfferKind kind, DefinitionBase target, int price, bool required, bool lent, string vendor)
            {
                var offer = make.Def<MarketOfferDefinition>(id, HubFolder, name, name, "offer", "patusan");
                offer.kind = kind;
                offer.target = target;
                offer.priceScrip = price;
                offer.required = required;
                offer.lentForContract = lent;
                offer.vendor = vendor;
                return offer;
            }

            c.NewIdentity = Offer("offer_new_identity", "New identity", OfferKind.Service, null, 500, true, false, "The forger, lower ring");
            c.TransponderReflash = Offer("offer_transponder_reflash", "Transponder reflash", OfferKind.Service, null, 300, true, false, "Dockmaster");
            c.RepairJ1M = Offer("offer_repair_j1m", "Repair J1M", OfferKind.Service, c.Prologue.J1M, 100, false, false, "Repair bay");
            c.SecondWorkerOffer = Offer("offer_second_worker", "Used J-series worker", OfferKind.Unit, c.SecondWorker, 300, false, false, "Android dealer");
            c.PicketOffer = Offer("offer_picket", "Picket (combat droid)", OfferKind.Unit, c.Picket, 500, false, true, "Doramin");

            var augmentOffers = new List<MarketOfferDefinition>(c.Augments.Count);
            foreach (var augment in c.Augments)
            {
                augmentOffers.Add(Offer($"offer_{augment.Id}", augment.DisplayName, OfferKind.Augment, augment, augment.PriceScrip, false, false, "Augment clinic"));
            }

            c.AugmentOffers = augmentOffers;
        }

        private static void BuildScripts(ColdSleepContent c, DefinitionMaker make)
        {
            var p = c.Prologue;

            WaveDefinition Wave(string id, string name, params WaveSpawn[] spawns)
            {
                var wave = make.Def<WaveDefinition>(id, ScriptFolder, name, name, "wave", "cold_sleep");
                wave.spawns = spawns;
                return wave;
            }

            c.ConstableNoise = Wave("wave_constable_noise", "Drawn by the fight",
                new WaveSpawn(p.Crawler, 3, "security_duct", 8f, 3f));

            // Trip 1 stream, tuned to about 1,400 rounds: inside one load of both sentries (2,000).
            c.Trip1Stream = Wave("wave_trip1_stream", "The haul (trip 1)",
                new WaveSpawn(p.Crawler, 150, LanePrefix + "far_vent", 0f, 1.2f),
                new WaveSpawn(p.Spitter, 7, LanePrefix + "gallery", 30f, 20f));

            c.Trip2Nest = Wave("wave_trip2_nest", "The nest in the hold",
                new WaveSpawn(p.Crawler, 8, "cryo_hold_nest", 0f, 0.5f));

            c.Trip2LeaperIntro = Wave("wave_trip2_leaper_intro", "The first Leaper",
                new WaveSpawn(c.Leaper, 1, LanePrefix + "barricade_jump", 0f, 0f));

            // Trip 2 stream, about 2,100 rounds: more than one load, so a worker must reload mid-stream.
            c.Trip2Stream = Wave("wave_trip2_stream", "The haul (trip 2)",
                new WaveSpawn(p.Crawler, 200, LanePrefix + "far_vent", 0f, 0.9f),
                new WaveSpawn(p.Spitter, 6, LanePrefix + "gallery", 25f, 25f),
                new WaveSpawn(c.Leaper, 11, LanePrefix + "near_vent", 15f, 14f));

            var trip1Beats = new List<MissionBeatDefinition>(12);
            var trip2Beats = new List<MissionBeatDefinition>(8);

            void Beat(List<MissionBeatDefinition> into, string prefix, int order, string slug, int parSeconds, BeatTrigger trigger, string argument,
                bool checkpoint, string title, string body, string teaches, ObjectiveEntry[] objectives, params WaveDefinition[] waves)
            {
                var beat = make.Def<MissionBeatDefinition>($"{prefix}_{order:00}_{slug}", ScriptFolder, title, teaches, "beat", "cold_sleep");
                beat.order = order;
                beat.targetCountdownSeconds = parSeconds;
                beat.trigger = trigger;
                beat.triggerArgument = argument ?? string.Empty;
                beat.isCheckpoint = checkpoint;
                beat.promptTitle = title;
                beat.promptBody = body;
                beat.teaches = teaches;
                beat.objectives = objectives ?? Array.Empty<ObjectiveEntry>();
                beat.startsWaves = waves ?? Array.Empty<WaveDefinition>();
                into.Add(beat);
            }

            ObjectiveEntry[] Obj(params (string id, string text, bool optional)[] entries)
            {
                var result = new ObjectiveEntry[entries.Length];
                for (var i = 0; i < entries.Length; i++)
                {
                    result[i] = new ObjectiveEntry(entries[i].id, entries[i].text, entries[i].optional);
                }

                return result;
            }

            const string t1 = "beat_cs1";
            Beat(trip1Beats, t1, 1, "dock", 1500, BeatTrigger.MissionStart, null, true,
                "The Torrens", "Dark for six years. The hive is Dormant. If you played The Ghost, the vow starts now.",
                "Omen cards in play", Obj(("obj_reach_hold", "Reach the cryo hold", false)));
            Beat(trip1Beats, t1, 2, "pods", 1440, BeatTrigger.EnterRoom, "room_cryo_hold", false,
                "Bolted and wired", "Disconnecting a pod takes 90 s and it's loud. Insulate the hold first.",
                "Reading a hazard before acting", Obj(("obj_find_insulation", "Find insulation", false)));
            Beat(trip1Beats, t1, 3, "security", 1380, BeatTrigger.PreviousBeatCompleted, null, false,
                "Only security can open it", "The panels are in the security store. Wake the security synth: bring a power cell from the Nellie.",
                "Plans with steps", Obj(("obj_bring_cell", "Bring a power cell to the security office", false)));
            Beat(trip1Beats, t1, 4, "revive", 1290, BeatTrigger.ItemDelivered, "itm_shuttle_power_cell", false,
                "Wake it up", "Repair the synth at the station (20 s).", "Reviving units",
                Obj(("obj_revive", "Revive the security synth", false)));
            Beat(trip1Beats, t1, 5, "theft", 1230, BeatTrigger.InteractionCompleted, "revive_constable", true,
                "Removal of colonial property is theft", "Constable opened the store, then saw what you're taking. Bring it down.",
                "A mini-boss; focus fire", Obj(("obj_defeat_constable", "Defeat Constable", false)), c.ConstableNoise);
            Beat(trip1Beats, t1, 6, "insulate", 1140, BeatTrigger.WaveCleared, "wave_constable_noise", false,
                "Line the hold", "Install the 8 acoustic panels. Two workers do it twice as fast.", "Room treatments",
                Obj(("obj_install_panels", "Insulate the cryo hold", false)));
            Beat(trip1Beats, t1, 7, "detach", 1050, BeatTrigger.InteractionCompleted, "rt_acoustic_insulation", false,
                "Contained", "Disconnect both pods. The meter says Contained: nothing outside can hear it.", "Contained noise",
                Obj(("obj_detach_pods", "Disconnect both pods", false)));
            Beat(trip1Beats, t1, 8, "ammo", 960, BeatTrigger.PreviousBeatCompleted, null, true,
                "Feed the printer", "Cut bulkhead panels on the ballast deck and print ammo. There are 10 panels for both trips: plan.",
                "Crafting from the hulk; planning ahead",
                Obj(("obj_print_ammo", "Print at least 3 ammo boxes", false), ("obj_load_sentries", "Load both sentries on the Lane", false)));
            Beat(trip1Beats, t1, 9, "haul", 780, BeatTrigger.PreviousBeatCompleted, null, false,
                "Moving it", "The pod is heavy and loud. The hive wakes; the vow window closes.", "Hauling noise; the stream",
                Obj(("obj_haul_pod", "Haul pod 1 to the docking collar", false)), c.Trip1Stream);
            Beat(trip1Beats, t1, 10, "reload", 600, BeatTrigger.EnterRoom, "room_lane", false,
                "Watch the counters", "A worker can reload a sentry mid-fight (5 s per box).", "Reloading under pressure",
                Obj(("obj_keep_firing", "Keep both sentries firing", false)));
            Beat(trip1Beats, t1, 11, "aboard", 300, BeatTrigger.ItemDelivered, "itm_cryo_pod", true,
                "One down", "Pod 1 is in the Cradle. Undock.", "Partial payout", Obj(("obj_undock", "Undock", false)));
            Beat(trip1Beats, t1, 12, "paid", 120, BeatTrigger.PreviousBeatCompleted, null, false,
                "600 Scrip", "Half the pay. Picket has learned enough to specialize.", "Payment; specialization",
                Obj(("obj_return_patusan", "Return to Patusan", false)));

            const string t2 = "beat_cs2";
            Beat(trip2Beats, t2, 1, "restless", 1500, BeatTrigger.MissionStart, null, true,
                "They're waiting", "The hive is Restless from the start: patrols doubled.", "A hive that remembers",
                Obj(("obj_reach_hold_2", "Reach the cryo hold", false)));
            Beat(trip2Beats, t2, 2, "nest", 1380, BeatTrigger.EnterRoom, "room_cryo_hold", false,
                "They moved in", "Crawlers have nested around the last pod.", "Clearing a room",
                Obj(("obj_clear_nest", "Clear the hold", false)), c.Trip2Nest);
            Beat(trip2Beats, t2, 3, "sleeper", 1320, BeatTrigger.WaveCleared, "wave_trip2_nest", false,
                "For later", "The sleeper is still in the third pod. Marlow logs its location.", "The rescue seed",
                Obj(("obj_log_sleeper", "Log the sleeper's pod", true)));
            Beat(trip2Beats, t2, 4, "ammo_2", 1200, BeatTrigger.PreviousBeatCompleted, null, true,
                "What's left", "Cut the remaining panels and print what you can.", "Planning across trips",
                Obj(("obj_print_ammo_2", "Load both sentries", false)));
            Beat(trip2Beats, t2, 5, "leaper", 1020, BeatTrigger.PreviousBeatCompleted, null, false,
                "It jumped", "A Leaper crouches, then jumps the barricade onto a sentry and knocks it over. A worker can set it up again (3 s).",
                "The Leaper", Obj(("obj_right_sentry", "Set the sentry back up", false)), c.Trip2LeaperIntro);
            Beat(trip2Beats, t2, 6, "haul_2", 900, BeatTrigger.PreviousBeatCompleted, null, false,
                "Last load", "Haul the second pod. Picket's specialization is what holds this.", "Specializations in play",
                Obj(("obj_haul_pod_2", "Haul pod 2 to the docking collar", false)), c.Trip2Stream);
            Beat(trip2Beats, t2, 7, "aboard_2", 300, BeatTrigger.ItemDelivered, "itm_cryo_pod", true,
                "Both pods", "Undock.", "Finishing a chain", Obj(("obj_undock_2", "Undock", false)));
            Beat(trip2Beats, t2, 8, "complete", 120, BeatTrigger.PreviousBeatCompleted, null, false,
                "Contract complete", "Doramin pays the rest, and mentions he has other work.", "Completing a contract",
                Obj(("obj_return_patusan_2", "Return to Patusan", false)));

            MissionScriptDefinition Script(string id, string name, List<MissionBeatDefinition> beats)
            {
                var script = make.Def<MissionScriptDefinition>(id, ScriptFolder, name, name, "mission", "cold_sleep");
                script.countdownSeconds = 1500f; // Par time, not a fail timer.
                script.countdownIsFailTimer = false;
                script.pauseCountdownDuringPrompts = true;
                script.beats = beats.ToArray();
                return script;
            }

            c.Trip1 = Script("mission_cold_sleep_1", "Cold Sleep (1/2)", trip1Beats);
            c.Trip2 = Script("mission_cold_sleep_2", "Cold Sleep (2/2)", trip2Beats);

            var contract = make.Def<ContractDefinition>("con_cold_sleep", HubFolder, "Cold Sleep",
                "Two cryo pods from the Torrens for Doramin.", "contract", "starter");
            contract.client = "Doramin";
            contract.hulkName = "Torrens";
            contract.deliverable = c.CryoPod;
            contract.payPerPart = new[] { PodPayPerTrip, PodPayPerTrip };
            contract.moduleGiven = c.CargoCradle;
            contract.itemsGiven = new[] { new ItemStack(p.SentryMk1, 2) };
            contract.partScripts = new[] { c.Trip1, c.Trip2 };
            c.Contract = contract;
        }
    }
}
