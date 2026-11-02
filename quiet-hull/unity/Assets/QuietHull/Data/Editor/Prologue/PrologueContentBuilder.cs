using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// The single source of truth for every prologue value. The numbers mirror the
    /// "Prologue demo level" tab of the design doc; change them here, regenerate, and the
    /// edit-mode tests check that the tuning targets still hold.
    /// </summary>
    public static class PrologueContentBuilder
    {
        public const string CatalogId = "cat_prologue";

        // Spawn point ids placed in the level scene.
        public const string LaneAPrefix = "lane_a_";
        public const string LaneBPrefix = "lane_b_";

        // Asset sub-folders.
        private const string MaterialsFolder = "Materials";
        private const string ItemsFolder = "Items";
        private const string CraftingFolder = "Crafting";
        private const string UnitsFolder = "Units";
        private const string WorldFolder = "World";
        private const string ScriptFolder = "Script";

        public static PrologueContent Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var maker = new DefinitionMaker(factory);
            var content = new PrologueContent();

            BuildMaterials(content, maker);
            BuildItems(content, maker);
            BuildCrafting(content, maker);
            BuildUnits(content, maker);
            BuildWorld(content, maker);
            BuildScript(content, maker);

            var catalog = factory.GetOrCreateCatalog(CatalogId);
            catalog.catalogId = CatalogId;
            catalog.definitions = new List<DefinitionBase>(maker.All);
            catalog.InvalidateLookup();

            content.Catalog = catalog;
            content.All = maker.All;
            return content;
        }

        private static void BuildMaterials(PrologueContent c, DefinitionMaker make)
        {
            c.Scrap = make.Def<MaterialDefinition>("mat_scrap", MaterialsFolder, "Scrap", "Loose metal and parts. Enough for simple builds.", "material");
            c.Plating = make.Def<MaterialDefinition>("mat_plating", MaterialsFolder, "Plating", "Armour plate and structural stock.", "material", "currency");
            c.Circuitry = make.Def<MaterialDefinition>("mat_circuitry", MaterialsFolder, "Circuitry", "Boards, wiring and sensors.", "material", "currency");
            c.Ordnance = make.Def<MaterialDefinition>("mat_ordnance", MaterialsFolder, "Ordnance", "Propellant, charges and casings.", "material", "currency");
        }

        private static void BuildItems(PrologueContent c, DefinitionMaker make)
        {
            var prod = make.Def<MeleeWeaponDefinition>("itm_shock_prod", ItemsFolder, "Shock prod",
                "Vashti-Kell riot baton. The only thing a Lifter-class android is authorised to carry.", "weapon", "melee", "starter");
            prod.damage = 20f;
            prod.attackIntervalSeconds = 1.2f;
            prod.range = 1.8f;
            prod.stunEveryNthHit = 3;
            prod.stunSeconds = 1.5f;
            prod.noiseOnUse = 4f;
            prod.heatOnUse = 3f;
            prod.lightOnUse = LightEmission.Dim;
            c.ShockProd = prod;

            var pistol = make.Def<RangedWeaponDefinition>("itm_service_pistol", ItemsFolder, "Service pistol",
                "Standard-issue officer's sidearm from the cockpit locker.", "weapon", "ranged");
            pistol.damagePerShot = 22f;
            pistol.fireIntervalSeconds = 0.33f;
            pistol.range = 15f;
            pistol.magazineSize = 12;
            pistol.reloadSeconds = 1.5f;
            pistol.unlimitedReserve = true;
            pistol.noiseOnUse = 8f;
            pistol.heatOnUse = 2f;
            pistol.lightOnUse = LightEmission.Bright;
            c.ServicePistol = pistol;

            var grenade = make.Def<ThrowableDefinition>("itm_stun_grenade", ItemsFolder, "Stun grenade",
                "Security-issue flash-stun charge. Loud.", "throwable", "stun");
            grenade.throwRange = 10f;
            grenade.effectRadius = 4f;
            grenade.fuseSeconds = 1.5f;
            grenade.stunSeconds = 4f;
            grenade.damage = 0f;
            grenade.maxStack = 3;
            grenade.noiseOnUse = 12f;
            grenade.heatOnUse = 4f;
            grenade.lightOnUse = LightEmission.Bright;
            c.StunGrenade = grenade;

            var welder = make.Def<ToolDefinition>("itm_arc_welder", ItemsFolder, "Arc welder",
                "Cuts bolts and seals doors. Bright and hot: things notice.", "tool");
            welder.range = 1.5f;
            welder.actions = new[] { new ToolAction(ToolActionKind.CutBolt, 8f), new ToolAction(ToolActionKind.SealDoor, 6f) };
            welder.noiseOnUse = 5f;
            welder.heatOnUse = 8f;
            welder.lightOnUse = LightEmission.Bright;
            c.ArcWelder = welder;

            var disassembler = make.Def<ToolDefinition>("itm_molecular_disassembler", ItemsFolder, "Molecular disassembler",
                "Takes matter apart along its seams. Clears debris; mines alloy. Quiet, but it runs warm.", "tool", "mining");
            disassembler.range = 3f;
            disassembler.actions = new[] { new ToolAction(ToolActionKind.RemoveDebris, 5f), new ToolAction(ToolActionKind.MineResource, 6f) };
            disassembler.noiseOnUse = 2f;
            disassembler.heatOnUse = 6f;
            disassembler.lightOnUse = LightEmission.Dim;
            c.Disassembler = disassembler;

            var cart = make.Def<CartDefinition>("itm_hand_cart", ItemsFolder, "Hand cart",
                "Four squealing wheels and a flatbed. Carries what J1M can't.", "vehicle", "crafted");
            cart.capacitySlots = 6;
            cart.maxHealth = 200f;
            cart.noiseMoving = 6f;
            cart.pushSpeed = 2.2f;
            c.HandCart = cart;

            var cylinder = make.Def<CargoDefinition>("itm_fuel_cylinder", ItemsFolder, "Fuel cylinder",
                "Pressurised shuttle fuel. Too heavy to carry by hand.", "cargo", "mission", "explosive");
            cylinder.slots = 1;
            cylinder.maxCondition = 100f;
            cylinder.conditionLossPerSecondByHeat = new[] { 0f, 0f, 0f, 0f };
            cylinder.explodesWhenDestroyed = true;
            cylinder.explosionDamage = 150f;
            cylinder.explosionRadius = 5f;
            c.FuelCylinder = cylinder;

            var cryoCell = make.Def<CargoDefinition>("itm_cryo_fuel_cell", ItemsFolder, "Cryo fuel cell",
                "Powers the shuttle's cryo systems. Ruptures if it gets warm.", "cargo", "mission", "heat_sensitive");
            cryoCell.slots = 1;
            cryoCell.maxCondition = 100f;
            cryoCell.conditionLossPerSecondByHeat = new[] { 0f, 1f, 3f, 6f };
            cryoCell.explodesWhenDestroyed = true;
            cryoCell.explosionDamage = 100f;
            cryoCell.explosionRadius = 4f;
            c.CryoCell = cryoCell;

            var alloy = make.Def<ResourceDefinition>("itm_grav_lattice_alloy", ItemsFolder, "Grav-lattice alloy",
                "Stripped from the grav-drive housing. Not to be salvaged by crew. Worth a new identity.", "resource", "exotic");
            alloy.slotsPerUnit = 1;
            alloy.scripValue = 400;
            alloy.premiumFromUnit = 5;
            alloy.premiumScripValue = 600;
            c.GravLatticeAlloy = alloy;

            var keycard = make.Def<KeyItemDefinition>("itm_keycard_valve_control", ItemsFolder, "Valve control keycard",
                "Chief engineer Okafor's card. Opens valve control.", "key", "story");
            keycard.opensDoorId = "door_valve_control";
            keycard.consumedOnUse = false;
            c.ValveKeycard = keycard;

            var sentry = make.Def<SentryDefinition>("itm_sentry_mk1", ItemsFolder, "Sentry Mk I",
                "A machine-gun sentry from a hacked fabricator pattern. Watch the counter.", "deployable", "sentry");
            sentry.maxHealth = 250f;
            sentry.armour = 20f;
            sentry.damagePerRound = 9f;
            sentry.roundsPerSecond = 8f;
            sentry.expectedHitRate = 0.6f;
            sentry.range = 18f;
            sentry.arcDegrees = 90f;
            sentry.ammoCapacity = 1000;
            sentry.deploySeconds = 3f;
            sentry.pickupSeconds = 5f;
            sentry.noiseOnUse = 10f;
            sentry.heatOnUse = 5f;
            sentry.lightOnUse = LightEmission.Bright;
            c.SentryMk1 = sentry;

            var ammo = make.Def<AmmunitionDefinition>("itm_sentry_ammo_box", ItemsFolder, "Sentry ammo box",
                "500 rounds for a Sentry Mk I.", "ammunition");
            ammo.rounds = 500;
            ammo.compatibleSentry = sentry;
            c.SentryAmmoBox = ammo;
        }

        private static void BuildCrafting(PrologueContent c, DefinitionMaker make)
        {
            var cartRecipe = make.Def<RecipeDefinition>("rcp_hand_cart", CraftingFolder, "Hand cart", "Weld a flatbed onto salvaged wheels.", "recipe");
            cartRecipe.output = c.HandCart;
            cartRecipe.outputCount = 1;
            cartRecipe.inputs = new[] { new MaterialCost(c.Scrap, 10) };
            cartRecipe.craftSeconds = 8f;
            cartRecipe.requiresBlueprint = false;
            c.HandCartRecipe = cartRecipe;

            var grenadeRecipe = make.Def<RecipeDefinition>("rcp_stun_grenade", CraftingFolder, "Stun grenade", "Security pattern. Needs the blueprint.", "recipe");
            grenadeRecipe.output = c.StunGrenade;
            grenadeRecipe.outputCount = 1;
            grenadeRecipe.inputs = new[] { new MaterialCost(c.Ordnance, 2), new MaterialCost(c.Circuitry, 1) };
            grenadeRecipe.craftSeconds = 4f;
            grenadeRecipe.requiresBlueprint = true;
            c.StunGrenadeRecipe = grenadeRecipe;

            var sentryRecipe = make.Def<RecipeDefinition>("rcp_sentry_mk1", CraftingFolder, "Sentry Mk I", "Unlocked by hacking the bay fabricator.", "recipe");
            sentryRecipe.output = c.SentryMk1;
            sentryRecipe.outputCount = 1;
            sentryRecipe.inputs = new[] { new MaterialCost(c.Plating, 6), new MaterialCost(c.Circuitry, 4), new MaterialCost(c.Ordnance, 2) };
            sentryRecipe.craftSeconds = 20f;
            sentryRecipe.requiresBlueprint = true;
            c.SentryRecipe = sentryRecipe;

            var ammoRecipe = make.Def<RecipeDefinition>("rcp_sentry_ammo_box", CraftingFolder, "Sentry ammo box", "Unlocked by hacking the bay fabricator.", "recipe");
            ammoRecipe.output = c.SentryAmmoBox;
            ammoRecipe.outputCount = 1;
            ammoRecipe.inputs = new[] { new MaterialCost(c.Ordnance, 2) };
            ammoRecipe.craftSeconds = 6f;
            ammoRecipe.requiresBlueprint = true;
            c.AmmoBoxRecipe = ammoRecipe;

            var blueprint = make.Def<BlueprintDefinition>("bp_stun_grenade", CraftingFolder, "Blueprint: stun grenade",
                "Found on the security officer. Teaches the grenade bench the pattern.", "blueprint");
            blueprint.unlocks = grenadeRecipe;
            c.StunGrenadeBlueprint = blueprint;

            var maintenance = make.Def<WorkstationDefinition>("ws_android_maintenance", CraftingFolder, "Android maintenance station",
                "Repairs worker androids and builds simple tools.", "workstation");
            maintenance.services = WorkstationService.RepairUnit | WorkstationService.Craft;
            maintenance.recipes = new[] { cartRecipe };
            maintenance.repairHealthPerSecond = 40f;
            maintenance.repairMaxSeconds = 6f;
            maintenance.repairIsFree = true;
            maintenance.requiresHack = false;
            maintenance.interactSeconds = 1f;
            c.MaintenanceStation = maintenance;

            var bench = make.Def<WorkstationDefinition>("ws_grenade_bench", CraftingFolder, "Security grenade bench",
                "Builds stun grenades from a known pattern. The bins are empty.", "workstation");
            bench.services = WorkstationService.Craft;
            bench.recipes = new[] { grenadeRecipe };
            bench.repairHealthPerSecond = 0f;
            bench.repairMaxSeconds = 0f;
            bench.interactSeconds = 1f;
            c.GrenadeBench = bench;

            var fabricator = make.Def<WorkstationDefinition>("ws_bay_fabricator", CraftingFolder, "Bay fabricator (hacked)",
                "A cargo-bay printer. Marlow's hack unlocked two military patterns.", "workstation", "fabricator");
            fabricator.services = WorkstationService.Fabricate;
            fabricator.recipes = new[] { sentryRecipe, ammoRecipe };
            fabricator.repairHealthPerSecond = 0f;
            fabricator.repairMaxSeconds = 0f;
            fabricator.requiresHack = true;
            fabricator.interactSeconds = 1.5f;
            c.BayFabricator = fabricator;
        }

        private static void BuildUnits(PrologueContent c, DefinitionMaker make)
        {
            var j1m = make.Def<UnitDefinition>("unit_j1m", UnitsFolder, "J1M",
                "Lifter-class worker android. Answers to Jim. Not built for this.", "unit", "worker", "android");
            j1m.maxHealth = 300f;
            j1m.armour = 10f;
            j1m.canBeDestroyed = false;
            j1m.wreckedSeconds = 8f;
            j1m.rebootHealthFraction = 0.3f;
            j1m.moveSpeed = 3f;
            j1m.noiseWalking = 2f;
            j1m.heatIdle = 2f;
            j1m.heatAfterExertion = 5f;
            j1m.exertionCooldownSeconds = 6f;
            j1m.lampRange = 8f;
            j1m.handCarrySlots = 1;
            j1m.abilities = UnitAbility.Move | UnitAbility.Interact | UnitAbility.Attack | UnitAbility.Patrol | UnitAbility.PushCart | UnitAbility.Throw;
            j1m.startingItems = Array.Empty<ItemDefinition>();
            c.J1M = j1m;

            var marlow = make.Def<UnitDefinition>("unit_marlow", UnitsFolder, "Warrant Officer Marlow",
                "Last officer of the Patna. Directly controlled in the finale, held to the bay doorway.", "unit", "commander");
            marlow.maxHealth = 120f;
            marlow.armour = 0f;
            marlow.canBeDestroyed = false;
            marlow.wreckedSeconds = 2f; // Downed: the finale restarts.
            marlow.rebootHealthFraction = 1f;
            marlow.moveSpeed = 3.5f;
            marlow.noiseWalking = 1f;
            marlow.heatIdle = 4f;
            marlow.heatAfterExertion = 5f;
            marlow.exertionCooldownSeconds = 6f;
            marlow.lampRange = 0f;
            marlow.handCarrySlots = 0;
            marlow.abilities = UnitAbility.Move | UnitAbility.Interact | UnitAbility.RepairShuttle | UnitAbility.HackFabricator | UnitAbility.FireWeapon;
            marlow.startingItems = new ItemDefinition[] { c.ServicePistol };
            c.Marlow = marlow;

            var crawler = make.Def<AlienDefinition>("alien_crawler", UnitsFolder, "Crawler",
                "Small, fast, travels through ducts. Hears and feels; no heat vision.", "alien");
            crawler.maxHealth = 40f;
            crawler.armour = 0f;
            crawler.moveSpeed = 4.5f;
            crawler.usesDucts = true;
            crawler.attack = new AttackProfile(AttackKind.Melee, 8f, 1f, 1.2f, 0f);
            crawler.targetPriority = new[] { TargetKind.NearestUnit, TargetKind.Cargo };
            crawler.senses = new SenseProfile(1f, 1f, 6f, 120f, 0f);
            crawler.signalRadius = 15f;
            crawler.signalCooldownSeconds = 6f;
            crawler.giveUpSeconds = 10f;
            crawler.defaultBehaviour = AlienBehaviour.Patrol;
            crawler.surgeCost = 2;
            crawler.chitinBounty = 2;
            crawler.experience = 2;
            c.Crawler = crawler;

            var spitter = make.Def<AlienDefinition>("alien_spitter", UnitsFolder, "Spitter",
                "Lobs acid over cover. Sees heat. Goes for sentries first.", "alien", "ranged");
            spitter.maxHealth = 110f;
            spitter.armour = 5f;
            spitter.moveSpeed = 1.8f;
            spitter.usesDucts = false;
            spitter.attack = new AttackProfile(AttackKind.Lob, 25f, 3f, 20f, 2f);
            spitter.targetPriority = new[] { TargetKind.Sentry, TargetKind.UnitInOpen, TargetKind.NearestUnit };
            spitter.senses = new SenseProfile(0.7f, 0f, 15f, 90f, 15f);
            spitter.signalRadius = 0f;
            spitter.signalCooldownSeconds = 0f;
            spitter.giveUpSeconds = 10f;
            spitter.defaultBehaviour = AlienBehaviour.HoldPosition;
            spitter.surgeCost = 4;
            spitter.chitinBounty = 3;
            spitter.experience = 4;
            c.Spitter = spitter;
        }

        private static void BuildWorld(PrologueContent c, DefinitionMaker make)
        {
            var crusher = make.Def<RoomAmbienceDefinition>("amb_ore_crusher_bay", WorldFolder, "Ore crusher bay", "Steady masking noise.", "ambience");
            crusher.pattern = AmbiencePattern.Steady;
            crusher.peakLevel = 6f;
            crusher.baseLevel = 6f;
            crusher.periodSeconds = 0f;
            crusher.peakSeconds = 0f;
            crusher.causeText = "The crusher jammed on overload and won't stop turning.";
            c.CrusherBay = crusher;

            var pumps = make.Def<RoomAmbienceDefinition>("amb_coolant_pump_hall", WorldFolder, "Coolant pump hall", "Pulsing masking noise: move on the beat.", "ambience");
            pumps.pattern = AmbiencePattern.Pulsing;
            pumps.peakLevel = 7f;
            pumps.baseLevel = 1f;
            pumps.periodSeconds = 4f;
            pumps.peakSeconds = 1.5f;
            pumps.causeText = "The pumps are cavitating as the reactor overheats; they hammer every few seconds.";
            c.PumpHall = pumps;

            var valves = make.Def<RoomAmbienceDefinition>("amb_valve_gallery", WorldFolder, "Valve gallery", "Burst masking noise: wait for the vent.", "ambience");
            valves.pattern = AmbiencePattern.Burst;
            valves.peakLevel = 10f;
            valves.baseLevel = 0f;
            valves.periodSeconds = 12f;
            valves.peakSeconds = 3f;
            valves.causeText = "Relief valves scream open to bleed reactor pressure.";
            c.ValveGallery = valves;

            var shelves = make.Def<LootCacheDefinition>("cache_maintenance_shelves", WorldFolder, "Maintenance shelves", "Scrap for the hand cart.", "cache");
            shelves.materials = new[] { new MaterialCost(c.Scrap, 10) };
            shelves.items = Array.Empty<ItemStack>();
            shelves.accessRequires = Array.Empty<ToolActionKind>();
            c.MaintenanceShelves = shelves;

            var corpse = make.Def<LootCacheDefinition>("cache_security_officer", WorldFolder, "Security officer",
                "Burst from the back. Their recorder: \"They mostly come out of the chest. Mostly.\"", "cache", "corpse", "story");
            corpse.materials = Array.Empty<MaterialCost>();
            corpse.items = new[] { new ItemStack(c.StunGrenade, 1), new ItemStack(c.StunGrenadeBlueprint, 1) };
            corpse.accessRequires = Array.Empty<ToolActionKind>();
            c.SecurityOfficerCorpse = corpse;

            var cage = make.Def<LootCacheDefinition>("cache_quartermaster_cage", WorldFolder, "Quartermaster's cage",
                "Bolted and barricaded. Everything for two sentries, plus a little spare.", "cache", "locked");
            cage.materials = new[] { new MaterialCost(c.Plating, 12), new MaterialCost(c.Circuitry, 8), new MaterialCost(c.Ordnance, 14) };
            cage.items = Array.Empty<ItemStack>();
            cage.accessRequires = new[] { ToolActionKind.CutBolt, ToolActionKind.RemoveDebris };
            c.QuartermasterCage = cage;

            var engineer = make.Def<LootCacheDefinition>("cache_chess_engineer", WorldFolder, "Chief engineer Okafor",
                "Torn half apart at the chess table in the crew mess. The board shows the winning move he never played: queen takes bishop.",
                "cache", "corpse", "story");
            engineer.materials = Array.Empty<MaterialCost>();
            engineer.items = new[] { new ItemStack(c.ValveKeycard, 1) };
            engineer.accessRequires = Array.Empty<ToolActionKind>();
            c.ChessEngineer = engineer;

            var dark = make.Def<HazardZoneDefinition>("hz_dark_reactor_corridor", WorldFolder, "Dark reactor corridor",
                "No power. J1M's lamp shows the way and shows J1M to everything else.", "hazard", "dark");
            dark.kind = HazardKind.Dark;
            dark.intensity = 1;
            dark.moveSpeedMultiplier = 1f;
            dark.damagePerSecond = 0f;
            dark.visionWithLamp = 8f;
            dark.visionWithoutLamp = 2f;
            dark.lampSpottedRangeMultiplier = 2f;
            dark.clearedAtInteractionId = "junction_reactor_corridor";
            dark.clearSeconds = 6f;
            dark.clearNoise = 4f;
            dark.requiresKey = null;
            dark.residualIntensity = 0;
            dark.residualMoveSpeedMultiplier = 1f;
            dark.residualDamagePerSecond = 0f;
            c.DarkReactorCorridor = dark;

            var flood = make.Def<HazardZoneDefinition>("hz_coolant_flood", WorldFolder, "Coolant junction flood",
                "A ruptured coolant main. Anything that walks in freezes before it reaches the far side. A frozen J1M is dragged back out by the level script.", "hazard", "cold");
            flood.kind = HazardKind.Cold;
            flood.intensity = 3;
            flood.moveSpeedMultiplier = 0.4f;
            flood.damagePerSecond = 20f;
            flood.visionWithLamp = 0f;
            flood.visionWithoutLamp = 0f;
            flood.lampSpottedRangeMultiplier = 1f;
            flood.clearedAtInteractionId = "valve_coolant_main";
            flood.clearSeconds = 5f;
            flood.clearNoise = 3f;
            flood.requiresKey = c.ValveKeycard;
            flood.residualIntensity = 1;
            flood.residualMoveSpeedMultiplier = 0.9f;
            flood.residualDamagePerSecond = 0f;
            c.CoolantFlood = flood;
        }

        private static void BuildScript(PrologueContent c, DefinitionMaker make)
        {
            WaveDefinition Wave(string id, string name, params WaveSpawn[] spawns)
            {
                var wave = make.Def<WaveDefinition>(id, ScriptFolder, name, name, "wave");
                wave.spawns = spawns;
                return wave;
            }

            var patrol = Wave("wave_corridor_patrol", "Corridor patrol",
                new WaveSpawn(c.Crawler, 1, "corridor_b_patrol", 0f, 0f));
            var firstContact = Wave("wave_first_contact", "First contact",
                new WaveSpawn(c.Crawler, 2, "corridor_b_north", 0f, 1.5f),
                new WaveSpawn(c.Crawler, 1, "duct_b1", 4f, 0f));
            var follower = Wave("wave_follower", "The follower",
                new WaveSpawn(c.Crawler, 1, "pump_hall_vent", 0f, 0f));
            var coldCorridor = Wave("wave_cold_corridor_spitter", "Spitter in the cold corridor",
                new WaveSpawn(c.Spitter, 1, "port_corridor_hold", 0f, 0f));
            var perimeter = Wave("wave_shuttle_perimeter", "Drawn by the repairs",
                new WaveSpawn(c.Crawler, 3, LaneAPrefix + "vent", 10f, 20f),
                new WaveSpawn(c.Crawler, 3, LaneBPrefix + "vent", 20f, 20f));

            // The stream: 100 Crawlers per lane in three waves, tuned so each sentry ends with
            // 10–20% of its 1,000 rounds. The lane A Spitter sits out of sentry range on the
            // gantry: J1M has to kill it (the lesson that sentries alone aren't enough).
            var stream1 = Wave("wave_stream_1", "The stream (1/3)",
                new WaveSpawn(c.Crawler, 30, LaneAPrefix + "vent", 0f, 1.6f),
                new WaveSpawn(c.Crawler, 30, LaneBPrefix + "vent", 0.8f, 1.6f));
            var stream2 = Wave("wave_stream_2", "The stream (2/3)",
                new WaveSpawn(c.Crawler, 35, LaneAPrefix + "vent", 0f, 1.3f),
                new WaveSpawn(c.Crawler, 35, LaneBPrefix + "vent", 0.6f, 1.3f),
                new WaveSpawn(c.Spitter, 1, "gantry_a_spitter", 12f, 0f));
            var stream3 = Wave("wave_stream_3", "The stream (3/3)",
                new WaveSpawn(c.Crawler, 35, LaneAPrefix + "vent", 0f, 1.1f),
                new WaveSpawn(c.Crawler, 35, LaneBPrefix + "vent", 0.5f, 1.1f));
            c.StreamWaves = new[] { stream1, stream2, stream3 };

            var miningGuard = Wave("wave_grav_housing", "Grav-drive housing nest",
                new WaveSpawn(c.Crawler, 4, "grav_housing_duct", 5f, 6f));
            var finale = Wave("wave_finale_bay_doors", "Through the bay doors",
                new WaveSpawn(c.Crawler, 24, "bay_door_left", 0f, 2f),
                new WaveSpawn(c.Crawler, 16, "bay_door_right", 3f, 3f),
                new WaveSpawn(c.Spitter, 2, "bay_door_right", 20f, 15f));

            // Three Crawlers standing perfectly still in the dark corridor: revealed when the power comes back.
            var still = Wave("wave_dark_corridor_still", "Standing in the dark",
                new WaveSpawn(c.Crawler, 3, "reactor_corridor_still", 0f, 0f));
            c.StillCrawlers = still;

            // Frozen mid-leap in the coolant junction; they thaw when the Patna turns to the sun.
            var thawing = Wave("wave_coolant_thaw", "The frozen ones",
                new WaveSpawn(c.Crawler, 3, "coolant_junction_frozen", 20f, 4f));
            c.ThawingCrawlers = thawing;

            var beats = new List<MissionBeatDefinition>(27);

            void Beat(int order, string slug, int countdownSeconds, BeatTrigger trigger, string argument, bool checkpoint,
                string promptTitle, string promptBody, string teaches, ObjectiveEntry[] objectives, params WaveDefinition[] waves)
            {
                var id = $"beat_{order:00}_{slug}";
                var beat = make.Def<MissionBeatDefinition>(id, ScriptFolder, promptTitle, teaches, "beat", "prologue");
                beat.order = order;
                beat.targetCountdownSeconds = countdownSeconds;
                beat.trigger = trigger;
                beat.triggerArgument = argument ?? string.Empty;
                beat.isCheckpoint = checkpoint;
                beat.promptTitle = promptTitle;
                beat.promptBody = promptBody;
                beat.teaches = teaches;
                beat.objectives = objectives ?? Array.Empty<ObjectiveEntry>();
                beat.startsWaves = waves ?? Array.Empty<WaveDefinition>();
                beats.Add(beat);
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

            // Act 1: the fuel run.
            Beat(1, "wake_j1m", 1800, BeatTrigger.MissionStart, null, true,
                "Your only crew", "Select J1M and move it. Marlow can't leave the cockpit yet.",
                "Selecting and moving a unit; the camera", Obj(("obj_move_j1m", "Move J1M out of the bay", false)));
            Beat(2, "armory", 1740, BeatTrigger.EnterRoom, "room_armory", false,
                "Authorised equipment", "Take the shock prod. Everything else is locked to security staff.",
                "Interacting; equipping", Obj(("obj_take_prod", "Take the shock prod", false)));
            Beat(3, "patrol", 1680, BeatTrigger.ItemPickedUp, "itm_shock_prod", false,
                "It hasn't seen you", "Watch its cone and the icons over its head. Wait for it to turn, or fight.",
                "Patrols, vision cones, sense icons", Obj(("obj_pass_patrol", "Get past the patrol", false)), patrol);
            Beat(4, "first_contact", 1620, BeatTrigger.EnterRoom, "room_corridor_b", false,
                "It called the others", "A shriek pulls every alien within 15 m. Fight with the prod; the third hit stuns.",
                "Melee; signalling; ducts", Obj(("obj_survive_contact", "Kill the Crawlers", false)), firstContact);
            Beat(5, "lights_out", 1560, BeatTrigger.EnterRoom, "room_reactor_corridor", false,
                "Lights out", "Your lamp lets you see, and lets them see you. Restore power at the junction box.",
                "Light as a sense; restoring power",
                Obj(("obj_restore_power", "Restore power at the junction", false), ("obj_walk_past", "Walk past without touching them", true)), still);
            Beat(6, "too_heavy", 1500, BeatTrigger.EnterRoom, "room_reactor", true,
                "Too heavy", "The fuel cylinders are too heavy to carry. J1M needs a cart.",
                "Why tools matter", Obj(("obj_find_cart", "Find a way to move the cylinders", false)));
            Beat(7, "maintenance", 1440, BeatTrigger.EnterRoom, "room_android_maintenance", false,
                "Patch yourself up", "Repair J1M at the station, then build a hand cart from the scrap on the shelves.",
                "Workstations; healing; crafting",
                Obj(("obj_repair", "Repair J1M", false), ("obj_craft_cart", "Craft a hand cart", false)));
            Beat(8, "squeal", 1350, BeatTrigger.CraftCompleted, "rcp_hand_cart", true,
                "You're loud now", "The wheels squeal. The ring around J1M is how far that carries.",
                "Your own noise", Obj(("obj_load_cylinders", "Load both fuel cylinders", false)));
            Beat(9, "masking", 1290, BeatTrigger.ItemPickedUp, "itm_fuel_cylinder", false,
                "Hide in the noise", "Loud rooms cover your noise. Crusher: always. Pumps: on the beat. Valves: on a burst.",
                "Masking: steady, pulsing, bursts", Obj(("obj_reach_cryo", "Take the loud route toward the cryo bay", false)));
            Beat(10, "follower", 1230, BeatTrigger.EnterRoom, "room_coolant_pump_hall", false,
                "Break contact", "It's following your sound. Get a door between you and wait.",
                "Breaking contact; giving up", Obj(("obj_lose_it", "Lose the Crawler", true)), follower);
            Beat(11, "flash_freeze", 1170, BeatTrigger.EnterRoom, "room_coolant_junction", false,
                "Flash freeze", "The coolant main has burst. Anything that walks in freezes. The valve room needs a keycard.",
                "Cold; reading a hazard", Obj(("obj_find_keycard", "Find a keycard for valve control", false)));
            Beat(12, "chess", 1110, BeatTrigger.EnterRoom, "room_crew_mess", false,
                "Queen takes bishop", "Chief engineer Okafor. His keycard is still on him.",
                "Key items", Obj(("obj_take_keycard", "Take Okafor's keycard", false)));
            Beat(13, "valve", 1050, BeatTrigger.ItemPickedUp, "itm_keycard_valve_control", true,
                "Shut it off", "Open valve control and shut the coolant main. The junction will stay cold, which the cryo cells will like.",
                "Switching off a hazard", Obj(("obj_shut_valve", "Shut the coolant valve", false)));
            Beat(14, "cryo_cells", 990, BeatTrigger.InteractionCompleted, "valve_coolant_main", false,
                "Handle with care", "Cryo cells lose condition when they get warm. Watch the bar on the cart.",
                "Cargo condition", Obj(("obj_load_cells", "Load both cryo fuel cells", false)));
            Beat(15, "sun", 930, BeatTrigger.ItemPickedUp, "itm_cryo_fuel_cell", false,
                "The Patna turns to the sun", "Starboard rooms are heating up. The coolant junction is still cold, but something in there is moving.",
                "Heat; picking a route", Obj(("obj_keep_cells", "Keep the cryo cells intact", false)), thawing);
            Beat(16, "security_officer", 840, BeatTrigger.EnterRoom, "room_security_checkpoint", false,
                "Mostly", "Search the officer: a stun grenade and its blueprint. The bench shows what it would need.",
                "Blueprints and recipes", Obj(("obj_search_officer", "Search the security officer", false)));
            Beat(17, "spitter", 780, BeatTrigger.EnterRoom, "room_port_corridor", false,
                "It sees heat", "The flame icon means it sees you through smoke. Stun it, then close in.",
                "Ranged enemies; heat vision; grenades", Obj(("obj_kill_spitter", "Kill the Spitter", false)), coldCorridor);

            // Act 2: the shuttle.
            Beat(18, "delivery", 690, BeatTrigger.ItemDelivered, "itm_cryo_fuel_cell", true,
                "Hold the perimeter", "Marlow is repairing the Nellie, and it's loud. Order J1M to patrol around the shuttle.",
                "Patrol orders; shuttle noise draws aliens", Obj(("obj_patrol", "Patrol the shuttle perimeter", false)), perimeter);
            Beat(19, "hack", 660, BeatTrigger.InteractionCompleted, "ws_bay_fabricator", false,
                "Two patterns", "Marlow cracked the fabricator: Sentry Mk I and ammo. The materials are in the Quartermaster's cage.",
                "Fabricators; tools", Obj(("obj_take_tools", "Take the welder and the disassembler", false)));
            Beat(20, "cage", 630, BeatTrigger.EnterRoom, "room_quartermaster_cage_door", false,
                "Locked out", "Cut the 4 bolts with the welder (bright, hot), then take apart the fallen rafters.",
                "Welding; disassembling; light and heat",
                Obj(("obj_cut_bolts", "Cut the bolts", false), ("obj_clear_rafters", "Clear the rafters", false), ("obj_haul_materials", "Bring the materials to the bay", false)));
            Beat(21, "sentries", 600, BeatTrigger.ItemDelivered, "cache_quartermaster_cage", true,
                "Hold the lanes", "Build two sentries and four ammo boxes. Place one on each lane.",
                "Sentries; lanes; ammo",
                Obj(("obj_build_sentries", "Fabricate 2 sentries", false), ("obj_build_ammo", "Fabricate 4 ammo boxes", false), ("obj_place_sentries", "Cover both lanes", false)));
            Beat(22, "liability", 600, BeatTrigger.CraftCompleted, "rcp_sentry_mk1", false,
                "Clause 14(c)", "WARDEN reads out the contract. Losing the Patna makes Marlow liable. Vanishing costs money.",
                "The extraction motive", Obj(("obj_hear_warden", "Listen to WARDEN", false)));
            Beat(23, "stream", 570, BeatTrigger.PreviousBeatCompleted, null, false,
                "They're coming", "The sentries hold the lanes. The Spitter on the gantry is out of their reach: send J1M.",
                "Sentries vs Spitters; the stream", Obj(("obj_hold_lanes", "Hold both lanes", false), ("obj_kill_gantry_spitter", "Kill the gantry Spitter", false)),
                stream1, stream2, stream3);

            // Act 3: the alloy and the launch.
            Beat(24, "alloy", 480, BeatTrigger.WaveCleared, "wave_stream_3", false,
                "Worth a new name", "Mine the grav-lattice alloy. Two units will do. Six would set you up. The clock is running.",
                "Mining; greed vs the timer",
                Obj(("obj_mine_two", "Mine 2 units of alloy", false), ("obj_mine_six", "Mine up to 6 units", true)), miningGuard);
            Beat(25, "return", 180, BeatTrigger.EnterRoom, "room_cargo_bay_b", true,
                "Last rounds", "The counters are nearly empty. Get the alloy aboard.",
                "Ammo matters", Obj(("obj_load_alloy", "Load the alloy", false)));
            Beat(26, "bay_doors", 120, BeatTrigger.ItemDelivered, "itm_grav_lattice_alloy", false,
                "Doorway", "Take the pistol. You're in direct control: hold the doorway until the doors close.",
                "The commander can fight in a pinch", Obj(("obj_hold_doorway", "Hold the bay doorway", false)), finale);
            Beat(27, "launch", 45, BeatTrigger.TimerReached, "45", false,
                "Launch", "Strap in.",
                "Launching", Obj(("obj_launch", "Launch the Nellie", false)));

            var warden = make.Def<AnnouncementScheduleDefinition>("ann_warden_patna", ScriptFolder, "WARDEN",
                "The Patna's AI. Calm, polite, corporate, and failing with the ship.", "announcements", "prologue");
            warden.speakerName = "WARDEN";
            warden.announcements = new[]
            {
                new Announcement(1800f, "Attention. Reactor containment failure in thirty minutes. All personnel proceed to designated lifeboats. Vashti-Kell thanks you for your service.", false),
                new Announcement(1500f, "Attention. Reactor containment failure in twenty-five minutes. All personnel proceed to designated lifeboats.", false),
                new Announcement(1200f, "Lifeboat two has launched. Lifeboat three has launched. Lifeboat one: launch aborted.", false),
                new Announcement(900f, "Fifteen minutes. Reminder: crew members are liable for company property left aboard.", false),
                new Announcement(600f, "Attention. Reactor containment failure in ten minutes. All per... all personnel proceed to designated lifeboats.", true),
                new Announcement(300f, "Five minutes.", false),
                new Announcement(240f, "Four minutes.", false),
                new Announcement(180f, "Three minutes.", true),
                new Announcement(120f, "Two minutes. Two. Two minutes.", true),
                new Announcement(60f, "One minute. This announcement will not be repeated.", false),
                new Announcement(10f, "Ten. Nine. Eight. Seven. Six. Five. Four. Three.", true)
            };
            c.Warden = warden;

            var mission = make.Def<MissionScriptDefinition>("mission_prologue_patna", ScriptFolder, "The last 30 minutes of the Patna",
                "Prologue and demo: escape the Patna before the reactor goes.", "mission", "prologue");
            mission.countdownSeconds = 1800f;
            mission.pauseCountdownDuringPrompts = true;
            mission.beats = beats.ToArray();
            c.Mission = mission;
        }

        /// <summary>Gets or creates definitions through the factory and keeps catalog order.</summary>
        private sealed class DefinitionMaker
        {
            private readonly IDefinitionFactory _factory;
            private readonly List<DefinitionBase> _all = new List<DefinitionBase>(96);

            public DefinitionMaker(IDefinitionFactory factory) => _factory = factory;

            public IReadOnlyList<DefinitionBase> All => _all;

            public T Def<T>(string id, string folder, string displayName, string description, params string[] tags) where T : DefinitionBase
            {
                var definition = _factory.GetOrCreate<T>(id, folder);
                definition.id = id;
                definition.displayName = displayName;
                definition.description = description;
                definition.tags = tags ?? Array.Empty<string>();
                _all.Add(definition);
                return definition;
            }
        }
    }
}
