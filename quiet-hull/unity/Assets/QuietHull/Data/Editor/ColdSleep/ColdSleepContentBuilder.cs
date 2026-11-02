using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Mission 2, "Cold Sleep": Patusan (the hub), the two-part cryo pod job on the Torrens, and the
    /// Judea side job that rewards the handgun conversion kit. Values mirror the "Mission 2: Cold Sleep"
    /// tab of the design doc. Shared definitions (materials, sentries, the handgun, Crawlers, Spitters,
    /// J1M) come from the prologue builder, so both catalogs reference the same assets.
    /// </summary>
    public static class ColdSleepContentBuilder
    {
        public const string CatalogId = "cat_cold_sleep";

        /// <summary>Spawn points on the Lane, the haul corridor both sentries cover.</summary>
        public const string LanePrefix = "lane_";

        // Level layout facts the tuning depends on (the scene must match).
        public const int LaneSegments = 6;
        public const int LaneBulkheadSegments = 4;
        public const int LaneConduitSegments = 2;
        public const int JudeaKillZoneSegments = 3;

        // Economy.
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
            var augs = AugmentCatalogBuilder.Build(factory);
            var make = new DefinitionMaker(factory);
            var c = new ColdSleepContent { Prologue = prologue, Augs = augs, Augments = augs.StandardLine() };
            c.Factions = FactionCatalogBuilder.Build(make);

            BuildMaterials(c, make);
            BuildWeapons(c, make);
            BuildGrowth(c, make);
            BuildUnits(c, make);
            BuildOmens(c, make);
            BuildWorld(c, make);
            BuildCrafting(c, make);
            c.Ordnance = OrdnanceCatalogBuilder.Build(factory, prologue, c.XenoalloyIngot);
            BuildHub(c, make);
            BuildTeasers(c, make);
            BuildScripts(c, make);

            c.Catalog = make.WriteCatalog(CatalogId);
            c.All = make.All;
            return c;
        }

        private static void BuildMaterials(ColdSleepContent c, DefinitionMaker make)
        {
            var chunk = make.Def<MaterialDefinition>("mat_amalgam_chunk", ItemsFolder, "Amalgam chunk",
                "Half-melted ship metal fused with alien acid. Break it down at a station.", "material", "amalgam");
            chunk.maxStack = 99;
            c.AmalgamChunk = chunk;

            var ingot = make.Def<MaterialDefinition>("mat_xenoalloy_ingot", ItemsFolder, "Xenoalloy ingot",
                "Refined amalgam: three chunks pressed into something a fabricator can trust.", "material", "amalgam");
            ingot.maxStack = 99;
            c.XenoalloyIngot = ingot;

            var myomer = make.Def<MaterialDefinition>("mat_synthetic_myomer", ItemsFolder, "Synthetic myomer bundle",
                "Military-grade artificial muscle from a security synth.", "material", "exotic");
            myomer.maxStack = 10;
            c.SyntheticMyomer = myomer;
        }

        private static void BuildWeapons(ColdSleepContent c, DefinitionMaker make)
        {
            var kit = make.Def<WeaponModDefinition>("itm_mod_pistol_conversion", ItemsFolder, "Micro-conversion kit",
                "Converts the military handgun to automatic fire, with recoil damping and three extended caseless magazines. Still no machine gun.",
                "weapon_mod", "automatic");
            kit.fitsWeapon = c.Prologue.ServicePistol;
            kit.fireMode = FireMode.Auto;
            kit.burstLength = 1;
            kit.fireIntervalMultiplier = 0.45f;  // 0.33 s → 0.15 s
            kit.damageMultiplier = 0.64f;        // lighter caseless loads: 22 → 14
            kit.magazineSize = 30;
            kit.reloadMultiplier = 1.33f;        // longer magazines: 1.5 s → 2 s
            kit.accuracyAdd = 0f;                // the damping cancels the spray
            kit.spareMagazines = 3;
            c.PistolConversionKit = kit;

            // Later gear, defined now so the locked specializations can name what they need.
            var rifle = make.Def<RangedWeaponDefinition>("itm_long_rifle", ItemsFolder, "Long rifle",
                "A Marksman's rifle. Slow, accurate, devastating on a weak point. Not found until later.", "weapon", "ranged", "long_rifle");
            rifle.damagePerShot = 60f;
            rifle.fireIntervalSeconds = 1.5f;
            rifle.range = 28f;
            rifle.magazineSize = 5;
            rifle.reloadSeconds = 2.5f;
            rifle.accuracy = 0.8f;
            rifle.noiseOnUse = 9f;
            rifle.heatOnUse = 3f;
            rifle.lightOnUse = LightEmission.Bright;
            c.LongRifle = rifle;

            var foam = make.Def<RangedWeaponDefinition>("itm_foam_projector", ItemsFolder, "Foam projector",
                "Sprays hardening foam. Hurts nothing; stops everything. Not found until later.", "weapon", "crowd_control_kit");
            foam.damagePerShot = 0f;
            foam.fireIntervalSeconds = 0.5f;
            foam.range = 14f;
            foam.magazineSize = 20;
            foam.reloadSeconds = 2f;
            foam.accuracy = 0.9f;
            foam.slowFraction = 0.4f;
            foam.slowSeconds = 3f;
            foam.noiseOnUse = 3f;
            foam.heatOnUse = 1f;
            foam.lightOnUse = LightEmission.None;
            c.FoamProjector = foam;
        }

        private static void BuildGrowth(ColdSleepContent c, DefinitionMaker make)
        {
            // The first choice is between two roles that work with what the player has now.
            var burst = make.Def<SpecializationDefinition>("spec_burst_gunner", GrowthFolder, "Burst Gunner",
                "Short, controlled bursts. Turns any gun into a swarm killer; grows into the Heavy Gunner.", "specialization");
            burst.archetype = Archetype.HeavyGunner;
            burst.unlockExperience = 50;
            burst.weaponOverride = null;
            burst.requiresEquipmentTag = string.Empty;
            burst.lockedReason = string.Empty;
            burst.passives = new[]
            {
                StatModifier.Add(UnitStat.Accuracy, 0.15f),
                StatModifier.Percent(UnitStat.ReloadTime, -0.25f),
                StatModifier.Percent(UnitStat.FireRate, 0.10f)
            };
            burst.abilityId = "ab_suppressing_fire";
            burst.abilityName = "Suppressing fire";
            burst.abilityDescription = "For 5 s, every alien hit in a 30° cone within 15 m is staggered and slowed 30%; a crouching Leaper is knocked out of its jump.";
            burst.abilityCooldownSeconds = 12f;
            burst.abilityDurationSeconds = 5f;
            burst.abilityRadius = 15f;
            burst.strongAgainst = "Swarms and Leapers. Best with the conversion kit, useful without it.";
            burst.weakAgainst = "Armoured targets; long reload windows.";
            c.BurstGunner = burst;

            var bulwark = make.Def<SpecializationDefinition>("spec_bulwark", GrowthFolder, "Bulwark",
                "Holds the line and regenerates faster than a small pack can hurt it.", "specialization");
            bulwark.archetype = Archetype.Bulwark;
            bulwark.unlockExperience = 50;
            bulwark.weaponOverride = null;
            bulwark.requiresEquipmentTag = string.Empty;
            bulwark.lockedReason = string.Empty;
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

            // Shown but locked until the gear exists, so nobody wastes the choice.
            var marksman = make.Def<SpecializationDefinition>("spec_marksman", GrowthFolder, "Marksman",
                "Huge critical hits at long range. Shines against elites.", "specialization");
            marksman.archetype = Archetype.Marksman;
            marksman.unlockExperience = 50;
            marksman.weaponOverride = c.LongRifle;
            marksman.requiresEquipmentTag = "long_rifle";
            marksman.lockedReason = "Needs a long rifle.";
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
            marksman.strongAgainst = "Elites, champions, Leapers, Spitters at range.";
            marksman.weakAgainst = "Swarms: one slow shot at a time.";
            c.Marksman = marksman;

            var controller = make.Def<SpecializationDefinition>("spec_controller", GrowthFolder, "Controller",
                "Slows the swarm so the sentries can do the killing.", "specialization");
            controller.archetype = Archetype.Controller;
            controller.unlockExperience = 50;
            controller.weaponOverride = c.FoamProjector;
            controller.requiresEquipmentTag = "crowd_control_kit";
            controller.lockedReason = "Needs a foam projector or a control drone.";
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
        }

        private static void BuildUnits(ColdSleepContent c, DefinitionMaker make)
        {
            var picket = make.Def<UnitDefinition>("unit_picket", UnitsFolder, "Picket",
                "A cheap Hollow Moon combat droid with the standard military handgun. No frills.", "unit", "combat", "synthetic");
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
            picket.startingItems = new ItemDefinition[] { c.Prologue.ServicePistol };
            picket.augmentSlots = new[]
            {
                AugmentSlot.Arms, AugmentSlot.Legs, AugmentSlot.Eyes, AugmentSlot.Ears,
                AugmentSlot.Lungs, AugmentSlot.Heart, AugmentSlot.Dermis, AugmentSlot.Brain
            };
            picket.specializationOptions = new[] { c.BurstGunner, c.Bulwark, c.Marksman, c.Controller };
            picket.combatRole = CombatRole.Combat;
            picket.defendRadius = 0f;
            picket.defendDamagePerSecond = 0f;
            picket.workSpeed = 0.8f;
            picket.augmentCapacityKg = 60f;
            picket.basePowerOutputWatts = 200f;
            picket.priceScrip = 500;
            picket.markCurve = c.Augs.MilitaryCurve;
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
            worker.abilities = j1m.Abilities | UnitAbility.SupplySentries | UnitAbility.Guard | UnitAbility.StaffStation | UnitAbility.Harvest | UnitAbility.AutoRecharge;
            worker.startingItems = Array.Empty<ItemDefinition>();
            worker.augmentSlots = new[] { AugmentSlot.Frame, AugmentSlot.Hardening, AugmentSlot.Hardening, AugmentSlot.Maintenance, AugmentSlot.ToolMount };
            worker.specializationOptions = new[] { c.Augs.Hauler, c.Augs.Miner, c.Augs.Constructor, c.Augs.Technician, c.Augs.Sapper };
            worker.charge = AugmentCatalog.WorkerCharge;
            worker.combatRole = CombatRole.DefendOnly;
            worker.defendRadius = 6f;
            worker.defendDamagePerSecond = 6f;
            worker.workSpeed = 1f;
            worker.augmentCapacityKg = 30f;
            worker.basePowerOutputWatts = 150f;
            worker.priceScrip = 300;
            worker.markCurve = c.Augs.WorkerCurve;
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
            leaper.amalgamWeight = 2f;
            leaper.heatTolerance = 1;
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
            constable.amalgamWeight = 0f; // Synthetic: no acid blood.
            constable.heatTolerance = 2;
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

            var amalgam = make.Def<AmalgamDefinition>("amalgam_acid_fused", WorldFolder, "Acid-fused amalgam",
                "Alien blood eats into the bulkheads where they die. Enough kills in one place leave a deposit you can mine.", "amalgam");
            amalgam.chunk = c.AmalgamChunk;
            amalgam.weightPerDeposit = 25f;
            amalgam.maxDepositsPerSegment = 3;
            amalgam.mineSeconds = 8f;
            amalgam.mineNoise = 2f;
            amalgam.baseYield = new[] { new MaterialCost(p.Scrap, 1) };
            amalgam.surfaceYields = new[]
            {
                new SurfaceYield(SurfaceKind.Bulkhead, p.Plating, 1),
                new SurfaceYield(SurfaceKind.Conduit, p.Circuitry, 1),
                new SurfaceYield(SurfaceKind.Munitions, p.Ordnance, 1),
                new SurfaceYield(SurfaceKind.Machinery, p.Scrap, 1)
            };
            amalgam.breakdownSeconds = 6f;
            c.Amalgam = amalgam;

            var pod = make.Def<CargoDefinition>("itm_cryo_pod", ItemsFolder, "Cryo pod",
                "A colonial sleep pod. With one, a person can leave. That's why they're so scarce.", "cargo", "contract", "oversize");
            pod.slots = 6;
            pod.maxCondition = 100f;
            pod.conditionLossPerSecondByHeat = new[] { 0f, 0.5f, 1.5f, 3f };
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
                "Opened by Constable. Riot gear, a crate of rounds and the acoustic panels.", "cache", "locked");
            store.materials = new[] { new MaterialCost(panel, 8), new MaterialCost(p.Ordnance, 4) };
            store.items = Array.Empty<ItemStack>();
            store.accessRequires = Array.Empty<ToolActionKind>();
            store.placedCount = 1;
            c.SecurityStore = store;

            var remains = make.Def<LootCacheDefinition>("cache_constable_remains", WorldFolder, "Constable's remains",
                "Strip it: the myomer is worth more than the rest of the ship.", "cache", "synthetic");
            remains.materials = new[] { new MaterialCost(c.SyntheticMyomer, 1), new MaterialCost(p.Circuitry, 2) };
            remains.items = Array.Empty<ItemStack>();
            remains.accessRequires = Array.Empty<ToolActionKind>();
            remains.placedCount = 1;
            c.ConstableRemains = remains;

            var bulkhead = make.Def<LootCacheDefinition>("cache_bulkhead_panel", WorldFolder, "Bulkhead panel",
                "Ballast-deck plating: the only loose scrap on the Torrens, shared by both trips.", "cache", "salvage");
            bulkhead.materials = new[] { new MaterialCost(p.Scrap, 6) };
            bulkhead.items = Array.Empty<ItemStack>();
            bulkhead.accessRequires = new[] { ToolActionKind.CutPanel };
            bulkhead.placedCount = 7;
            c.BulkheadPanel = bulkhead;

            var coal = make.Def<HazardZoneDefinition>("hz_judea_coal_fire", WorldFolder, "Smouldering bunker",
                "The Judea's coal bunker has been burning for years. Vent it to space to put it out.", "hazard", "heat");
            coal.kind = HazardKind.Heat;
            coal.intensity = 2;
            coal.moveSpeedMultiplier = 0.85f;
            coal.damagePerSecond = 4f;
            coal.visionWithLamp = 0f;
            coal.visionWithoutLamp = 0f;
            coal.lampSpottedRangeMultiplier = 1f;
            coal.clearedAtInteractionId = "vent_coal_bunker";
            coal.clearSeconds = 5f;
            coal.clearNoise = 8f;
            coal.requiresKey = null;
            coal.residualIntensity = 1;
            coal.residualMoveSpeedMultiplier = 1f;
            coal.residualDamagePerSecond = 0f;
            c.JudeaCoalFire = coal;
        }

        private static void BuildCrafting(ColdSleepContent c, DefinitionMaker make)
        {
            var p = c.Prologue;

            var ammo = make.Def<RecipeDefinition>("rcp_sentry_ammo_salvage", WorldFolder, "Sentry ammo box (salvage)",
                "Prints rounds from scrap.", "recipe");
            ammo.output = p.SentryAmmoBox;
            ammo.outputCount = 1;
            ammo.inputs = new[] { new MaterialCost(p.Scrap, 6) };
            ammo.itemInputs = Array.Empty<ItemStack>();
            ammo.minimumStationTier = 1;
            ammo.craftSeconds = 10f;
            ammo.requiresBlueprint = false;
            c.SalvageAmmoRecipe = ammo;

            var ingot = make.Def<RecipeDefinition>("rcp_xenoalloy_ingot", WorldFolder, "Xenoalloy ingot",
                "Press three amalgam chunks into one ingot.", "recipe");
            ingot.output = c.XenoalloyIngot;
            ingot.outputCount = 1;
            ingot.inputs = new[] { new MaterialCost(c.AmalgamChunk, 3) };
            ingot.itemInputs = Array.Empty<ItemStack>();
            ingot.minimumStationTier = 1;
            ingot.craftSeconds = 12f;
            ingot.requiresBlueprint = false;
            c.XenoalloyRecipe = ingot;

            var conversion = make.Def<RecipeDefinition>("rcp_pistol_conversion", WorldFolder, "Micro-conversion kit",
                "Needs the blueprint from the Judea.", "recipe");
            conversion.output = c.PistolConversionKit;
            conversion.outputCount = 1;
            conversion.inputs = new[] { new MaterialCost(p.Circuitry, 3), new MaterialCost(p.Plating, 2), new MaterialCost(c.XenoalloyIngot, 1) };
            conversion.itemInputs = Array.Empty<ItemStack>();
            conversion.minimumStationTier = 1;
            conversion.craftSeconds = 20f;
            conversion.requiresBlueprint = true;
            c.PistolConversionRecipe = conversion;

            var arms = make.Def<RecipeDefinition>("rcp_aug_arms_mk2", WorldFolder, "Servo arms Mk II",
                "Military pattern for the Mk II servo arms. Needs a healthy T2 fabricator, which neither the Nellie nor the Torrens has.", "recipe");
            arms.output = c.Augs.Synth(AugmentSlot.Arms, AugmentLine.Standard);
            arms.outputMark = Mark.MkII;
            arms.outputCount = 1;
            arms.inputs = new[] { new MaterialCost(p.Circuitry, 6), new MaterialCost(c.XenoalloyIngot, 4), new MaterialCost(c.SyntheticMyomer, 1) };
            arms.itemInputs = new[] { new ItemStack(p.GravLatticeAlloy, 1) };
            arms.minimumStationTier = 2;
            arms.craftSeconds = 60f;
            arms.requiresBlueprint = true;
            c.ServoArmsMk2Recipe = arms;

            var bpConversion = make.Def<BlueprintDefinition>("bp_pistol_conversion", WorldFolder, "Blueprint: micro-conversion kit",
                "From the Judea's armoury locker.", "blueprint");
            bpConversion.unlocks = conversion;
            c.PistolConversionBlueprint = bpConversion;

            var bpArms = make.Def<BlueprintDefinition>("bp_aug_arms_mk2", WorldFolder, "Blueprint: servo arms Mk II",
                "Doramin's bonus for the pods. Gather the materials; find a T2 fabricator.", "blueprint");
            bpArms.unlocks = arms;
            c.ServoArmsMk2Blueprint = bpArms;

            var torrens = make.Def<WorkstationDefinition>("ws_torrens_fabricator", WorldFolder, "Torrens fabricator (T1)",
                "Old, slow, still works. Prints ammo and breaks down amalgam.", "workstation", "fabricator");
            torrens.services = WorkstationService.Fabricate | WorkstationService.Breakdown;
            torrens.tier = 1;
            torrens.recipes = new[] { ammo, ingot };
            torrens.repairHealthPerSecond = 0f;
            torrens.repairMaxSeconds = 0f;
            torrens.requiresHack = false;
            torrens.interactSeconds = 1f;
            c.TorrensFabricator = torrens;

            var workshop = make.Def<WorkstationDefinition>("ws_nellie_workshop", WorldFolder, "Nellie workshop (T1)",
                "The shuttle's own fabricator. Marlow kept the sentry pattern from the Patna.", "workstation", "fabricator", "shuttle");
            workshop.services = WorkstationService.Fabricate | WorkstationService.Breakdown | WorkstationService.RepairUnit;
            workshop.tier = 1;
            workshop.recipes = new[] { p.SentryRecipe, ammo, ingot, conversion };
            workshop.repairHealthPerSecond = 20f;
            workshop.repairMaxSeconds = 30f;
            workshop.repairIsFree = false;
            workshop.requiresHack = false;
            workshop.interactSeconds = 1f;
            c.NellieWorkshop = workshop;

            var cradle = make.Def<WorkstationDefinition>("ws_nellie_cradle", WorldFolder, "Nellie charging cradle",
                "One bay, always safe, always far from the fighting.", "workstation", "charging", "shuttle");
            cradle.services = WorkstationService.Recharge;
            cradle.tier = 1;
            cradle.recipes = Array.Empty<RecipeDefinition>();
            cradle.repairHealthPerSecond = 0f;
            cradle.repairMaxSeconds = 0f;
            cradle.chargeBays = 1;
            cradle.rechargeMultiplier = 1f;
            cradle.interactSeconds = 1f;
            c.NellieCradle = cradle;

            var bay = make.Def<WorkstationDefinition>("ws_torrens_charging_bay", WorldFolder, "Torrens charging bay",
                "Three cradles next to the Lane. Welded shut and in vacuum: cut it open, restore the air, then it charges fast.",
                "workstation", "charging", "forward");
            bay.services = WorkstationService.Recharge;
            bay.tier = 1;
            bay.recipes = Array.Empty<RecipeDefinition>();
            bay.repairHealthPerSecond = 0f;
            bay.repairMaxSeconds = 0f;
            bay.chargeBays = 3;
            bay.rechargeMultiplier = 1.5f;
            bay.interactSeconds = 1f;
            bay.accessRequires = new[] { ToolActionKind.CutBolt };
            bay.requiresKey = null;
            bay.requiresAtmosphere = true;
            bay.atmospherePanelId = "panel_charging_bay_life_support";
            bay.atmosphereRestoreSeconds = 20f;
            c.TorrensChargingBay = bay;
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

        /// <summary>
        /// The teaser shelf at Patusan: a blueprint for every synth family (4 per body slot) and every
        /// worker module (at least 4 per work slot), all on show and none for sale yet. Each says what
        /// unlocks it: faction standing or a licence the player can't have in mission 2, and a price
        /// most of which is beyond what the mission pays.
        /// </summary>
        private static void BuildTeasers(ColdSleepContent c, DefinitionMaker make)
        {
            const string folder = "ColdSleep/Hub/Teasers";
            var p = c.Prologue;
            var offers = new List<MarketOfferDefinition>(64);

            var f = c.Factions;
            Requirement[] None() => Array.Empty<Requirement>();
            Requirement[] One(params Requirement[] r) => r;

            (int multiplier, int tier, string requirement, MaterialCost[] inputs, Requirement[] all, Requirement[] any) Terms(AugmentDefinition augment)
            {
                var worker = augment.IsWorkerModule;
                switch (augment.Line)
                {
                    case AugmentLine.Assault:
                        return worker
                            ? (4, 2, "Needs a Vashti-Kell industrial licence (or forged papers) and Drifters' Union standing: Friendly.",
                                new[] { new MaterialCost(p.Plating, 8), new MaterialCost(c.XenoalloyIngot, 2) },
                                One(Requirement.Faction(f.DriftersUnion, Standing.Friendly), Requirement.Holds(f.IndustrialLicence)), None())
                            : (9, 2, "Military pattern: needs a Lane Authority licence, or Hollow Moon standing: Trusted.",
                                new[] { new MaterialCost(p.Plating, 8), new MaterialCost(p.Ordnance, 2), new MaterialCost(c.XenoalloyIngot, 2) },
                                None(), One(Requirement.Holds(f.LaneMilitaryLicence), Requirement.Faction(f.HollowMoon, Standing.Trusted)));
                    case AugmentLine.Fine:
                        return (worker ? 5 : 10, 1, "Precision pattern: needs Drifters' Union standing: Friendly.",
                            new[] { new MaterialCost(p.Circuitry, 6), new MaterialCost(c.XenoalloyIngot, 1) },
                            One(Requirement.Faction(f.DriftersUnion, Standing.Friendly)), None());
                    case AugmentLine.Shadow:
                        return worker
                            ? (8, 3, "Contraband: needs Hollow Moon standing: Trusted. Confiscated in Lane Authority scans.",
                                new[] { new MaterialCost(p.Circuitry, 6), new MaterialCost(c.XenoalloyIngot, 3), new MaterialCost(c.SyntheticMyomer, 1) },
                                One(Requirement.Faction(f.HollowMoon, Standing.Trusted)), None())
                            : (8, 3, "Contraband: needs Hollow Moon standing: Trusted and Quiet Choir standing: Revered. Confiscated in Lane Authority scans.",
                                new[] { new MaterialCost(p.Circuitry, 6), new MaterialCost(c.XenoalloyIngot, 3), new MaterialCost(c.SyntheticMyomer, 1) },
                                One(Requirement.Faction(f.HollowMoon, Standing.Trusted), Requirement.Faction(f.QuietChoir, Standing.Revered)), None());
                    default:
                        return worker
                            ? (5, 1, "Needs Drifters' Union standing: Friendly.",
                                new[] { new MaterialCost(p.Plating, 4), new MaterialCost(p.Circuitry, 4) },
                                One(Requirement.Faction(f.DriftersUnion, Standing.Friendly)), None())
                            : (12, 1, "Needs Hollow Moon standing: Friendly.",
                                new[] { new MaterialCost(p.Plating, 4), new MaterialCost(p.Circuitry, 4) },
                                One(Requirement.Faction(f.HollowMoon, Standing.Friendly)), None());
                }
            }

            void Teaser(AugmentDefinition augment)
            {
                var terms = Terms(augment);
                var key = augment.Id;

                var recipe = make.Def<RecipeDefinition>($"rcp_{key}", folder, augment.DisplayName, $"Mk I pattern for {augment.DisplayName}.", "recipe", "teaser");
                recipe.output = augment;
                recipe.outputMark = Mark.MkI;
                recipe.outputCount = 1;
                recipe.inputs = terms.inputs;
                recipe.itemInputs = Array.Empty<ItemStack>();
                recipe.minimumStationTier = terms.tier;
                recipe.craftSeconds = 45f;
                recipe.requiresBlueprint = true;

                var blueprint = make.Def<BlueprintDefinition>($"bp_{key}", folder, $"Blueprint: {augment.DisplayName}", augment.Description, "blueprint", "teaser");
                blueprint.unlocks = recipe;

                var offer = make.Def<MarketOfferDefinition>($"offer_bp_{key}", folder, $"Blueprint: {augment.DisplayName}",
                    augment.Description, "offer", "patusan", "teaser");
                offer.kind = OfferKind.Blueprint;
                offer.target = blueprint;
                offer.priceScrip = augment.PriceScrip * terms.multiplier;
                offer.required = false;
                offer.lentForContract = false;
                offer.vendor = augment.Line == AugmentLine.Shadow ? "The back room" : augment.IsWorkerModule ? "Drifters' Union yard" : "Blueprint broker";
                offer.teaserOnly = true;
                offer.unlockRequirement = terms.requirement;
                offer.requiresAll = terms.all;
                offer.requiresAnyOf = terms.any;
                offers.Add(offer);
            }

            for (var slot = AugmentSlot.Arms; slot <= AugmentSlot.Brain; slot++)
            {
                foreach (AugmentLine line in Enum.GetValues(typeof(AugmentLine)))
                {
                    Teaser(c.Augs.Synth(slot, line));
                }
            }

            foreach (var module in c.Augs.WorkerModules)
            {
                Teaser(module);
            }

            // Advanced mines and the hidden compartment join the teaser shelf; the basic mines are for sale.
            var mineOffers = new List<MarketOfferDefinition>(3);
            foreach (var blueprint in c.Ordnance.MineBlueprints)
            {
                var mine = (MineDefinition)blueprint.Unlocks.Output;
                var basic = mine == c.Ordnance.Frag || mine == c.Ordnance.Stun || mine == c.Ordnance.Lure;
                var offer = make.Def<MarketOfferDefinition>($"offer_{blueprint.Id}", folder, blueprint.DisplayName, mine.Description, "offer", "patusan", "mine");
                offer.kind = OfferKind.Blueprint;
                offer.target = blueprint;
                offer.priceScrip = basic ? (mine == c.Ordnance.Lure ? 400 : mine == c.Ordnance.Frag ? 600 : 700) : 3500;
                offer.required = false;
                offer.lentForContract = false;
                offer.vendor = "Blueprint broker";
                offer.teaserOnly = !basic;
                offer.unlockRequirement = basic ? string.Empty
                    : mine == c.Ordnance.Shredder ? "Military pattern: needs a Lane Authority licence, or Hollow Moon standing: Trusted."
                    : mine == c.Ordnance.Bloodhound ? "Needs Hollow Moon standing: Trusted."
                    : "Needs Drifters' Union standing: Friendly.";
                offer.requiresAll = basic ? None()
                    : mine == c.Ordnance.Shredder ? None()
                    : mine == c.Ordnance.Bloodhound ? One(Requirement.Faction(f.HollowMoon, Standing.Trusted))
                    : One(Requirement.Faction(f.DriftersUnion, Standing.Friendly));
                offer.requiresAnyOf = mine == c.Ordnance.Shredder
                    ? One(Requirement.Holds(f.LaneMilitaryLicence), Requirement.Faction(f.HollowMoon, Standing.Trusted))
                    : None();
                (basic ? mineOffers : offers).Add(offer);
            }

            var compartmentOffer = make.Def<MarketOfferDefinition>("offer_mod_hidden_compartment", folder, "Hidden compartment",
                c.Ordnance.HiddenCompartment.Description, "offer", "patusan", "module", "contraband");
            compartmentOffer.kind = OfferKind.ShuttleModule;
            compartmentOffer.target = c.Ordnance.HiddenCompartment;
            compartmentOffer.priceScrip = c.Ordnance.HiddenCompartment.PriceScrip;
            compartmentOffer.required = false;
            compartmentOffer.lentForContract = false;
            compartmentOffer.vendor = "The back room";
            compartmentOffer.teaserOnly = true;
            compartmentOffer.unlockRequirement = "Contraband: needs Hollow Moon standing: Trusted.";
            compartmentOffer.requiresAll = One(Requirement.Faction(f.HollowMoon, Standing.Trusted));
            compartmentOffer.requiresAnyOf = None();
            offers.Add(compartmentOffer);

            c.MineOffers = mineOffers;
            c.TeaserOffers = offers;
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

            // Trip 1: about 1,400 rounds, inside one load of both sentries (2,000).
            c.Trip1Stream = Wave("wave_trip1_stream", "The haul (trip 1)",
                new WaveSpawn(p.Crawler, 150, LanePrefix + "far_vent", 0f, 1.2f),
                new WaveSpawn(p.Spitter, 7, LanePrefix + "gallery", 30f, 20f));

            c.Trip2Nest = Wave("wave_trip2_nest", "The nest in the hold",
                new WaveSpawn(p.Crawler, 8, "cryo_hold_nest", 0f, 0.5f));

            c.Trip2LeaperIntro = Wave("wave_trip2_leaper_intro", "The first Leaper",
                new WaveSpawn(c.Leaper, 1, LanePrefix + "barricade_jump", 0f, 0f));

            // Trip 2: the hive is aware and never stops. About 2,550 rounds: more than a load, so
            // the team must keep feeding the sentries from bulkheads and the amalgam the kills leave.
            c.Trip2Stream = Wave("wave_trip2_stream", "The haul (trip 2)",
                new WaveSpawn(p.Crawler, 260, LanePrefix + "far_vent", 0f, 0.8f),
                new WaveSpawn(p.Spitter, 8, LanePrefix + "gallery", 25f, 22f),
                new WaveSpawn(c.Leaper, 11, LanePrefix + "near_vent", 15f, 16f));

            // The Judea: a small kill zone in a conduit corridor, enough amalgam for the first ingot.
            c.JudeaWaves = Wave("wave_judea", "The Judea's crew",
                new WaveSpawn(p.Crawler, 70, "judea_conduit_vent", 0f, 1.3f),
                new WaveSpawn(p.Spitter, 4, "judea_bunker_door", 20f, 20f));

            ObjectiveEntry[] Obj(params (string id, string text, bool optional)[] entries)
            {
                var result = new ObjectiveEntry[entries.Length];
                for (var i = 0; i < entries.Length; i++)
                {
                    result[i] = new ObjectiveEntry(entries[i].id, entries[i].text, entries[i].optional);
                }

                return result;
            }

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

            var trip1 = new List<MissionBeatDefinition>(12);
            const string t1 = "beat_cs1";
            Beat(trip1, t1, 1, "dock", 1500, BeatTrigger.MissionStart, null, true,
                "The Torrens", "Dark for six years. The hive is Dormant. If you played The Ghost, the vow starts now.",
                "Omen cards in play", Obj(("obj_reach_hold", "Reach the cryo hold", false)));
            Beat(trip1, t1, 2, "pods", 1440, BeatTrigger.EnterRoom, "room_cryo_hold", false,
                "Bolted and wired", "Disconnecting a pod takes 90 s and it's loud. Insulate the hold first.",
                "Reading a hazard before acting", Obj(("obj_find_insulation", "Find insulation", false)));
            Beat(trip1, t1, 3, "security", 1380, BeatTrigger.PreviousBeatCompleted, null, false,
                "Only security can open it", "The panels are in the security store. Wake the security synth: bring a power cell from the Nellie.",
                "Plans with steps", Obj(("obj_bring_cell", "Bring a power cell to the security office", false)));
            Beat(trip1, t1, 4, "revive", 1290, BeatTrigger.ItemDelivered, "itm_shuttle_power_cell", false,
                "Wake it up", "Repair the synth at the station (20 s).", "Reviving units",
                Obj(("obj_revive", "Revive the security synth", false)));
            Beat(trip1, t1, 5, "theft", 1230, BeatTrigger.InteractionCompleted, "revive_constable", true,
                "Removal of colonial property is theft", "Constable opened the store, then saw what you're taking. Bring it down, then strip it.",
                "A mini-boss; focus fire", Obj(("obj_defeat_constable", "Defeat Constable", false), ("obj_strip_constable", "Take the myomer", true)), c.ConstableNoise);
            Beat(trip1, t1, 6, "insulate", 1140, BeatTrigger.WaveCleared, "wave_constable_noise", false,
                "Line the hold", "Install the 8 acoustic panels. Two workers do it twice as fast.", "Room treatments",
                Obj(("obj_install_panels", "Insulate the cryo hold", false)));
            Beat(trip1, t1, 7, "detach", 1050, BeatTrigger.InteractionCompleted, "rt_acoustic_insulation", false,
                "Contained", "Disconnect both pods. The meter says Contained: nothing outside can hear it.", "Contained noise",
                Obj(("obj_detach_pods", "Disconnect both pods", false)));
            Beat(trip1, t1, 8, "ammo", 960, BeatTrigger.PreviousBeatCompleted, null, true,
                "Feed the printer", "Cut bulkhead panels and print ammo. 7 panels must last both trips, and they are not enough on their own: plan.",
                "Crafting from the hulk; planning ahead",
                Obj(("obj_print_ammo", "Print at least 3 ammo boxes", false), ("obj_load_sentries", "Load both sentries on the Lane", false)));
            Beat(trip1, t1, 9, "haul", 780, BeatTrigger.PreviousBeatCompleted, null, false,
                "Moving it", "The pod is heavy and loud. The hive wakes; the vow window closes.", "Hauling noise; the stream",
                Obj(("obj_haul_pod", "Haul pod 1 to the docking collar", false)), c.Trip1Stream);
            Beat(trip1, t1, 10, "amalgam", 600, BeatTrigger.EnterRoom, "room_lane", false,
                "What their blood leaves", "Where they die, their acid eats the bulkheads. Mine the deposits; break them down for scrap and parts.",
                "Amalgam; kill zones as mines", Obj(("obj_mine_amalgam", "Mine an amalgam deposit", true), ("obj_keep_firing", "Keep both sentries firing", false)));
            Beat(trip1, t1, 11, "aboard", 300, BeatTrigger.ItemDelivered, "itm_cryo_pod", true,
                "One down", "Pod 1 is in the Cradle. Undock.", "Partial payout", Obj(("obj_undock", "Undock", false)));
            Beat(trip1, t1, 12, "paid", 120, BeatTrigger.PreviousBeatCompleted, null, false,
                "600 Scrip", "Half the pay. Picket has learned enough to specialize.", "Payment; specialization",
                Obj(("obj_return_patusan", "Return to Patusan", false)));

            var judea = new List<MissionBeatDefinition>(6);
            const string tj = "beat_jud";
            Beat(judea, tj, 1, "dock", 900, BeatTrigger.MissionStart, null, true,
                "The Judea", "A small collier, burning for years. The armoury officer's locker is past the bunker.",
                "A short job", Obj(("obj_reach_bunker", "Reach the coal bunker", false)));
            Beat(judea, tj, 2, "bunker", 840, BeatTrigger.EnterRoom, "room_judea_bunker", false,
                "Still burning", "Heat 2. Vent the bunker to space to put it out. It's loud.", "Heat; venting",
                Obj(("obj_vent", "Vent the coal bunker", false)));
            Beat(judea, tj, 3, "crew", 720, BeatTrigger.InteractionCompleted, "vent_coal_bunker", true,
                "Something heard that", "Hold the conduit corridor. Where they die, the amalgam forms.", "Kill zones as mines",
                Obj(("obj_hold_conduit", "Hold the conduit corridor", false)), c.JudeaWaves);
            Beat(judea, tj, 4, "mine", 480, BeatTrigger.WaveCleared, "wave_judea", false,
                "Conduit amalgam", "Mine the deposits. Conduit amalgam breaks down into Circuitry.", "Surface yields",
                Obj(("obj_mine_three", "Mine 3 amalgam chunks", false)));
            Beat(judea, tj, 5, "locker", 360, BeatTrigger.EnterRoom, "room_judea_armoury", false,
                "The armoury locker", "A blueprint: a micro-conversion kit for the military handgun.", "Blueprints as rewards",
                Obj(("obj_take_blueprint", "Take the blueprint", false)));
            Beat(judea, tj, 6, "home", 120, BeatTrigger.ItemPickedUp, "bp_pistol_conversion", true,
                "Back to the Nellie", "Press an ingot and build the kit in the Nellie's workshop.", "Crafting a weapon mod",
                Obj(("obj_craft_kit", "Craft the micro-conversion kit", true)));

            var trip2 = new List<MissionBeatDefinition>(8);
            const string t2 = "beat_cs2";
            Beat(trip2, t2, 1, "aware", 1500, BeatTrigger.MissionStart, null, true,
                "They're waiting", "The hive knows you're coming. It starts Restless and never stops sending.", "A hive that remembers",
                Obj(("obj_reach_hold_2", "Reach the cryo hold", false)));
            Beat(trip2, t2, 2, "nest", 1380, BeatTrigger.EnterRoom, "room_cryo_hold", false,
                "They moved in", "Crawlers have nested around the last pod.", "Clearing a room",
                Obj(("obj_clear_nest", "Clear the hold", false)), c.Trip2Nest);
            Beat(trip2, t2, 3, "sleeper", 1320, BeatTrigger.WaveCleared, "wave_trip2_nest", false,
                "For later", "The sleeper is still in the third pod. Marlow logs its location.", "The rescue seed",
                Obj(("obj_log_sleeper", "Log the sleeper's pod", true)));
            Beat(trip2, t2, 4, "foothold", 1200, BeatTrigger.PreviousBeatCompleted, null, true,
                "Push the foothold", "Cut what's left of the bulkheads, mine the trip 1 kill zone, print. A third sentry is possible if you have the parts.",
                "Planning across trips; amalgam", Obj(("obj_load_2", "Load both sentries", false), ("obj_third_sentry", "Build a third sentry", true)));
            Beat(trip2, t2, 5, "leaper", 1020, BeatTrigger.PreviousBeatCompleted, null, false,
                "It jumped", "A Leaper crouches, then jumps the barricade onto a sentry and knocks it over. A worker can set it up again (3 s).",
                "The Leaper", Obj(("obj_right_sentry", "Set the sentry back up", false)), c.Trip2LeaperIntro);
            Beat(trip2, t2, 6, "haul_2", 900, BeatTrigger.PreviousBeatCompleted, null, false,
                "Last load", "Haul the second pod. Keep the sentries fed; mine as you go.", "Specializations in play",
                Obj(("obj_haul_pod_2", "Haul pod 2 to the docking collar", false)), c.Trip2Stream);
            Beat(trip2, t2, 7, "aboard_2", 300, BeatTrigger.ItemDelivered, "itm_cryo_pod", true,
                "Both pods", "Undock.", "Finishing a chain", Obj(("obj_undock_2", "Undock", false)));
            Beat(trip2, t2, 8, "complete", 120, BeatTrigger.PreviousBeatCompleted, null, false,
                "Contract complete", "Doramin pays the rest and adds a blueprint you can't build yet: \"Find a better fabricator.\"",
                "Completing a contract; the next goal", Obj(("obj_return_patusan_2", "Return to Patusan", false)));

            MissionScriptDefinition Script(string id, string name, List<MissionBeatDefinition> beats, float parSeconds)
            {
                var script = make.Def<MissionScriptDefinition>(id, ScriptFolder, name, name, "mission", "cold_sleep");
                script.countdownSeconds = parSeconds; // Par time, not a fail timer.
                script.countdownIsFailTimer = false;
                script.pauseCountdownDuringPrompts = true;
                script.beats = beats.ToArray();
                return script;
            }

            c.Trip1 = Script("mission_cold_sleep_1", "Cold Sleep (1/2)", trip1, 1500f);
            c.Judea = Script("mission_judea", "The Judea", judea, 900f);
            c.Trip2 = Script("mission_cold_sleep_2", "Cold Sleep (2/2)", trip2, 1500f);

            var contract = make.Def<ContractDefinition>("con_cold_sleep", HubFolder, "Cold Sleep",
                "Two cryo pods from the Torrens for Doramin.", "contract", "starter");
            contract.client = "Doramin";
            contract.hulkName = "Torrens";
            contract.deliverable = c.CryoPod;
            contract.payPerPart = new[] { PodPayPerTrip, PodPayPerTrip };
            contract.moduleGiven = c.CargoCradle;
            contract.itemsGiven = new[] { new ItemStack(p.SentryMk1, 2) };
            contract.partScripts = new[] { c.Trip1, c.Trip2 };
            contract.completionBlueprints = new[] { c.ServoArmsMk2Blueprint };
            contract.completionStanding = new[] { StandingChange.AtLeast(c.Factions.HollowMoon, Standing.Friendly) };
            c.Contract = contract;

            var judeaContract = make.Def<ContractDefinition>("con_judea", HubFolder, "The Judea",
                "A side job from the board: recover an armoury locker's contents from a burning collier.", "contract", "side");
            judeaContract.client = "Patusan board";
            judeaContract.hulkName = "Judea";
            judeaContract.deliverable = null;
            judeaContract.payPerPart = new[] { 250 };
            judeaContract.moduleGiven = null;
            judeaContract.itemsGiven = Array.Empty<ItemStack>();
            judeaContract.partScripts = new[] { c.Judea };
            judeaContract.completionBlueprints = Array.Empty<BlueprintDefinition>();
            judeaContract.completionStanding = new[] { StandingChange.By(c.Factions.DriftersUnion, 1) };
            c.JudeaContract = judeaContract;

            var locker = make.Def<LootCacheDefinition>("cache_judea_armoury", WorldFolder, "Judea armoury locker",
                "The blueprint, and the parts to build it once you've pressed an ingot.", "cache", "reward");
            locker.materials = new[] { new MaterialCost(p.Circuitry, 3), new MaterialCost(p.Plating, 2) };
            locker.items = new[] { new ItemStack(c.PistolConversionBlueprint, 1) };
            locker.accessRequires = Array.Empty<ToolActionKind>();
            locker.placedCount = 1;
            c.JudeaArmoury = locker;
        }
    }
}
