using System;
using System.Collections.Generic;
using System.Linq;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Mission 3, "The Tremolino": the hunt for a T2 fabricator on a hulk tumbling through sunlight and
    /// an asteroid belt. The first semi-open mission: a real hive clock instead of scripted waves.
    /// Values mirror the "Mission 3: The Tremolino" tab of the design doc.
    /// </summary>
    public static class TremolinoContentBuilder
    {
        public const string CatalogId = "cat_tremolino";

        public const string ConsolePoweredEvent = "console_powered";
        public const string NestDisturbedEvent = "nest_disturbed";

        private const string Folder = "Tremolino";

        public static TremolinoContent Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var previous = ColdSleepContentBuilder.Build(factory);
            var (sun, belt) = SunwheelSampleBuilder.Build(factory);
            var make = new DefinitionMaker(factory);
            var c = new TremolinoContent { Previous = previous, Sun = sun, Belt = belt };

            BuildAliens(c, make);
            BuildWorld(c, make);
            BuildOmens(c, make);
            BuildStory(c, make);
            BuildHub(c, make);
            BuildScript(c, make);

            c.Catalog = make.WriteCatalog(CatalogId);
            c.All = make.All;
            return c;
        }

        private static void BuildAliens(TremolinoContent c, DefinitionMaker make)
        {
            var husk = make.Def<AlienDefinition>("alien_husk", Folder, "Husk",
                "The Tremolino's crew. Slow, many and noisy: they give the swarm away.", "alien", "fodder");
            husk.maxHealth = 60f;
            husk.armour = 0f;
            husk.moveSpeed = 2f;
            husk.usesDucts = false;
            husk.attack = new AttackProfile(AttackKind.Melee, 6f, 1f, 1.2f, 0f);
            husk.targetPriority = new[] { TargetKind.NearestUnit, TargetKind.Door };
            husk.senses = new SenseProfile(0.8f, 0.5f, 8f, 120f, 0f);
            husk.signalRadius = 0f;
            husk.giveUpSeconds = 20f;
            husk.defaultBehaviour = AlienBehaviour.Patrol;
            husk.surgeCost = 1;
            husk.chitinBounty = 1;
            husk.experience = 1;
            husk.amalgamWeight = 1f;
            husk.heatTolerance = 1;
            c.Husk = husk;

            var bloat = make.Def<AlienDefinition>("alien_bloat", Folder, "Bloat",
                "A Husk swollen with acid. Eats doors; bursts when it dies. A dead Bloat is a mine.", "alien", "exploder");
            bloat.maxHealth = 220f;
            bloat.armour = 10f;
            bloat.moveSpeed = 1.2f;
            bloat.usesDucts = false;
            bloat.attack = new AttackProfile(AttackKind.Melee, 8f, 1f, 1.2f, 0f);
            bloat.targetPriority = new[] { TargetKind.Door, TargetKind.NearestUnit };
            bloat.senses = new SenseProfile(1f, 0.5f, 6f, 120f, 0f);
            bloat.signalRadius = 0f;
            bloat.giveUpSeconds = 20f;
            bloat.defaultBehaviour = AlienBehaviour.Patrol;
            bloat.surgeCost = 5;
            bloat.chitinBounty = 4;
            bloat.experience = 5;
            bloat.amalgamWeight = 4f;
            bloat.heatTolerance = 2;
            bloat.deathBurstDamage = 40f;
            bloat.deathBurstRadius = 3f;
            bloat.deathBurstEatsDoors = true;
            bloat.doorDamagePerSecond = 15f;
            c.Bloat = bloat;
        }

        private static void BuildWorld(TremolinoContent c, DefinitionMaker make)
        {
            var cs = c.Previous;
            var p = cs.Prologue;
            var o = cs.Ordnance;

            var core = make.Def<KeyItemDefinition>("itm_regulator_core", Folder, "Regulator core",
                "The T2 fabricator's missing heart, pulled and hidden in the cryo vault by the last crew.", "key", "story");
            core.opensDoorId = "ws_tremolino_fabricator";
            core.consumedOnUse = true;
            c.RegulatorCore = core;

            var fabricator = make.Def<WorkstationDefinition>("ws_tremolino_fabricator", Folder, "Tremolino fabricator (T2)",
                "A stolen T2 fabricator, damaged. Install the regulator core and it makes Mk III–IV parts and T2 ordnance.", "workstation", "fabricator", "t2");
            fabricator.services = WorkstationService.Fabricate | WorkstationService.Breakdown;
            fabricator.tier = 2;
            fabricator.recipes = new[]
            {
                cs.ServoArmsMk2Recipe, p.SentryRecipe, cs.SalvageAmmoRecipe, cs.XenoalloyRecipe,
                o.MineRecipes.Single(r => r.Output == o.Shredder), o.MineRecipes.Single(r => r.Output == o.Bloodhound)
            };
            fabricator.repairHealthPerSecond = 0f;
            fabricator.repairMaxSeconds = 0f;
            fabricator.requiresKey = core;
            fabricator.interactSeconds = 2f;
            c.T2Fabricator = fabricator;

            var radiation = make.Def<HazardZoneDefinition>("hz_tremolino_deep_hold", Folder, "Deep hold radiation",
                "The reactor's unshielded flank. Strip the housing with the belt (hold facing 3) or send a worker with radiation lining.", "hazard", "radiation");
            radiation.kind = HazardKind.Radiation;
            radiation.intensity = 3;
            radiation.moveSpeedMultiplier = 1f;
            radiation.damagePerSecond = 8f;
            radiation.clearedAtInteractionId = "belt:reactor_housing";
            radiation.clearSeconds = 1f;
            radiation.clearNoise = 9f;
            radiation.residualIntensity = 1;
            radiation.residualMoveSpeedMultiplier = 1f;
            radiation.residualDamagePerSecond = 0f;
            c.DeepHoldRadiation = radiation;

            LootCacheDefinition Cache(string id, string name, string description, MaterialCost[] materials, ItemStack[] items, params string[] tags)
            {
                var cache = make.Def<LootCacheDefinition>(id, Folder, name, description, tags.Length > 0 ? tags : new[] { "cache" });
                cache.materials = materials;
                cache.items = items;
                cache.accessRequires = Array.Empty<ToolActionKind>();
                cache.placedCount = 1;
                return cache;
            }

            BlueprintDefinition Blueprint(MineDefinition mine) => o.MineBlueprints.Single(b => b.Unlocks.Output == mine);

            c.Armoury = Cache("cache_tremolino_armoury", "Tremolino armoury",
                "Open at facings 0 and 1. Rounds, mines, and the Shredder pattern.",
                new[] { new MaterialCost(p.Ordnance, 6) },
                new[] { new ItemStack(o.Frag, 4), new ItemStack(o.Stun, 2), new ItemStack(Blueprint(o.Shredder), 1) }, "cache", "armoury");
            c.PartsStore = Cache("cache_tremolino_parts", "Parts store",
                "Plating and Circuitry, and old amalgam from a fight nobody survived.",
                // Review fix 2: enough here for all four ingots, so the Mk II craft can't fail on mission 2 choices.
                new[] { new MaterialCost(p.Plating, 8), new MaterialCost(p.Circuitry, 6), new MaterialCost(cs.AmalgamChunk, 12) },
                // Review 2: one unit of alloy, so the Mk II arms never depend on the optional deep hold.
                new[] { new ItemStack(p.GravLatticeAlloy, 1) }, "cache", "parts");
            c.CryoVault = Cache("cache_tremolino_cryo_vault", "Cryo vault",
                "Open only at facing 2. The regulator core, and the smugglers' ledger.",
                Array.Empty<MaterialCost>(), new[] { new ItemStack(core, 1) }, "cache", "story");
            c.SecuritySynthRemains = Cache("cache_tremolino_security_synth", "Dead security synth",
                "On the crew deck, torn open by the crew it was guarding. Its myomer is intact.",
                new[] { new MaterialCost(cs.SyntheticMyomer, 1), new MaterialCost(p.Circuitry, 2) }, Array.Empty<ItemStack>(), "cache", "synthetic");

            c.DeepHold = Cache("cache_tremolino_deep_hold", "Deep hold",
                "Radiation 3 until the reactor housing is stripped. Three units of alloy and a Bloodhound pattern.",
                Array.Empty<MaterialCost>(), new[] { new ItemStack(p.GravLatticeAlloy, 3), new ItemStack(Blueprint(o.Bloodhound), 1) }, "cache", "optional");

            var hive = make.Def<HiveClockDefinition>("hive_tremolino", Folder, "Tremolino hive",
                "Dormant until the console wakes it; Hunt once the nest is disturbed or at 25 minutes.", "hive");
            hive.restlessOnEvent = ConsolePoweredEvent;
            hive.restlessAfterSeconds = 600f;
            hive.huntOnEvent = NestDisturbedEvent;
            hive.huntAfterSeconds = 1500f;
            hive.patrolsByStage = new[] { 3, 5, 8 };
            hive.surgeBudgetPerMinuteByStage = new[] { 0f, 8f, 18f };
            c.Hive = hive;

            var outpost = make.Def<OutpostDefinition>("outpost_tremolino", Folder, "The Tremolino outpost",
                "The T2 fabricator stays aboard. Dock again to craft T2 recipes, while the belt lasts.", "outpost");
            outpost.hulkName = "Tremolino";
            outpost.workstation = fabricator;
            outpost.sunExposure = c.Sun;
            outpost.belt = c.Belt;
            outpost.visitBudget = 6;
            outpost.dockingConsumables = 5;
            outpost.returnStage = HiveStage.Restless;
            c.Outpost = outpost;
        }

        private static void BuildOmens(TremolinoContent c, DefinitionMaker make)
        {
            var flare = make.Def<OmenCardDefinition>("omen_solar_flare", Folder, "Solar Flare",
                "Heat +1 on the hulk; fire affixes and Plating in the loot.", "omen", "condition", "reward");
            flare.family = OmenFamily.Condition;
            flare.rarity = Rarity.Common;
            flare.commonThreat = 2;
            flare.effects = new[]
            {
                new OmenEffect(OmenEffectKind.ConditionIntensity, OmenSide.Handicap, ScalingRule.Linear, 1f),
                new OmenEffect(OmenEffectKind.AffixBias, OmenSide.Benefit, ScalingRule.Chance, 0.30f),
                new OmenEffect(OmenEffectKind.CurrencyPercent, OmenSide.Benefit, ScalingRule.Linear, 50f)
            };
            flare.legendaryTwist = "Every fire item gets +1 socket.";
            c.SolarFlare = flare;

            var quiet = make.Def<OmenCardDefinition>("omen_quiet_decks", Folder, "Quiet Decks",
                "Half the patrols in Stages 1–2; a quarter less loot.", "omen", "expedition", "reward");
            quiet.family = OmenFamily.Expedition;
            quiet.rarity = Rarity.Common;
            quiet.commonThreat = -3;
            quiet.effects = new[]
            {
                new OmenEffect(OmenEffectKind.PatrolReduction, OmenSide.Benefit, ScalingRule.Reduction, 0.5f),
                new OmenEffect(OmenEffectKind.LootPercent, OmenSide.Handicap, ScalingRule.Linear, -25f)
            };
            quiet.legendaryTwist = "No patrols in fabricator rooms.";
            c.QuietDecks = quiet;

            c.OmenRewards = new[] { flare, quiet };
        }

        private static void BuildStory(TremolinoContent c, DefinitionMaker make)
        {
            var f = c.Factions;
            var ledger = make.Def<StoryChoiceDefinition>("choice_tremolino_ledger", Folder, "The smugglers' ledger",
                "The Tremolino carried a stolen alien egg for the Hollow Moon. It leaked. Doramin knew.", "story", "choice");
            ledger.prompt = "The ledger shows the Tremolino carried a live specimen stolen from a Vashti-Kell research ship, " +
                            "bought by the Hollow Moon to sell to the company's rivals. It leaked. That's the nest. Doramin knew.";
            ledger.options = new[]
            {
                new StoryOption
                {
                    optionId = "doramin",
                    label = "Return it to Doramin",
                    consequence = "Hollow Moon standing rises to Trusted. The Lane Authority marks you as an associate.",
                    standingChanges = new[] { StandingChange.AtLeast(f.HollowMoon, Standing.Trusted), StandingChange.By(f.LaneAuthority, -1) },
                    licencesGranted = Array.Empty<LicenceDefinition>(),
                    scrip = 0,
                    flags = new[] { "ledger_returned" }
                },
                new StoryOption
                {
                    optionId = "lane",
                    label = "Sell it to the Lane Authority",
                    consequence = "2,000 Scrip and a military licence. The Hollow Moon turns Hostile, and someone will come for the Nellie.",
                    standingChanges = new[] { StandingChange.AtMost(f.HollowMoon, Standing.Hostile), StandingChange.By(f.LaneAuthority, 1) },
                    licencesGranted = new[] { f.LaneMilitaryLicence },
                    scrip = 2000,
                    flags = new[] { "ledger_sold", "hollow_moon_reprisal" }
                },
                new StoryOption
                {
                    optionId = "keep",
                    label = "Keep it",
                    consequence = "Nothing now. Both factions keep asking; the Quiet Choir collects secrets.",
                    standingChanges = Array.Empty<StandingChange>(),
                    licencesGranted = Array.Empty<LicenceDefinition>(),
                    scrip = 0,
                    flags = new[] { "ledger_kept" }
                }
            };
            c.Ledger = ledger;
        }

        private static void BuildHub(TremolinoContent c, DefinitionMaker make)
        {
            var rumour = make.Def<MarketOfferDefinition>("offer_rumour_tremolino", Folder, "Rumour: the Tremolino",
                "Where the Tremolino is tumbling, and what she was carrying.", "offer", "patusan");
            rumour.kind = OfferKind.Service;
            rumour.target = null;
            rumour.priceScrip = 150;
            rumour.required = true;
            rumour.vendor = "Drifters' Union";
            c.Rumour = rumour;

            var lining = c.Previous.Augs.HardeningModules.Single(m => m.Id == "wmod_hard_rad");
            var liningOffer = make.Def<MarketOfferDefinition>("offer_wmod_hard_rad", Folder, lining.DisplayName,
                "Surplus radiation lining from the Drifters' yard: lets one worker into the deep hold early.", "offer", "patusan");
            liningOffer.kind = OfferKind.Augment;
            liningOffer.target = lining;
            liningOffer.priceScrip = lining.PriceScrip;
            liningOffer.vendor = "Drifters' Union yard";
            c.RadiationLining = liningOffer;
        }

        private static void BuildScript(TremolinoContent c, DefinitionMaker make)
        {
            var beats = new List<MissionBeatDefinition>(10);

            void Beat(int order, string slug, int parSeconds, BeatTrigger trigger, string argument, bool checkpoint,
                string title, string body, string teaches, params (string id, string text, bool optional)[] objectives)
            {
                var beat = make.Def<MissionBeatDefinition>($"beat_tr_{order:00}_{slug}", Folder, title, teaches, "beat", "tremolino");
                beat.order = order;
                beat.targetCountdownSeconds = parSeconds;
                beat.trigger = trigger;
                beat.triggerArgument = argument ?? string.Empty;
                beat.isCheckpoint = checkpoint;
                beat.promptTitle = title;
                beat.promptBody = body;
                beat.teaches = teaches;
                beat.objectives = objectives.Select(o => new ObjectiveEntry(o.id, o.text, o.optional)).ToArray();
                beat.startsWaves = Array.Empty<WaveDefinition>();
                beats.Add(beat);
            }

            Beat(1, "dock", 3000, BeatTrigger.MissionStart, null, true, "The Tremolino",
                "Tumbling through the Placid Drift. The hive is Dormant. Scout before you wake it.", "A semi-open hulk",
                ("obj_scout", "Scout the crew decks and the bridge", true));
            Beat(2, "console", 2820, BeatTrigger.EnterRoom, "room_bridge", false, "Dead stick",
                "The attitude console has no power. The junction is in the thruster gallery. Powering it will wake the hive.", "Restoring power; waking the hive",
                ("obj_power_console", "Power the attitude console", false));
            Beat(3, "first_choice", 2640, BeatTrigger.InteractionCompleted, ConsolePoweredEvent, true, "Before you turn",
                "At this facing the armoury and the charging bay are open. At the next two the bay bakes shut.", "Planning around the sun",
                ("obj_armoury", "Take what you need from the armoury", true),
                ("obj_charging_bay", "Unweld and repressurize the charging bay", true));
            Beat(4, "turn_one", 2400, BeatTrigger.InteractionCompleted, "console_turn_1", false, "The fabricator",
                "The fabrication deck has cooled. The fabricator's regulator core is missing.", "Sun-locked rooms",
                ("obj_find_core", "Find the regulator core", false), ("obj_kill_zone", "Set up the cargo bay", false));
            Beat(5, "turn_two", 2100, BeatTrigger.InteractionCompleted, "console_turn_2", true, "Into the sun",
                "The cryo vault opens; the nest turns into the sun and burns. What survives comes down the duct.", "The sun as a weapon",
                ("obj_core", "Take the regulator core", false), ("obj_ledger", "Read the ledger", false),
                ("obj_vent_nest", "Hold this facing until the belt vents the nest", true));
            Beat(6, "directive", 2000, BeatTrigger.InteractionCompleted, "console_turn_2", false, "Company specimens at risk",
                "J1M has stopped. Its old firmware counts the eggs as company property: it's walking into the burning nest to shield them. Order it back.",
                "Old firmware; your crew isn't only yours",
                ("obj_recall_j1m", "Order J1M back", false));
            Beat(7, "repair", 1500, BeatTrigger.ItemPickedUp, "itm_regulator_core", false, "Old machines",
                "Install the core, then craft. Choose which facing to hold while you defend.", "T2 crafting",
                ("obj_install_core", "Repair the fabricator", false), ("obj_craft_arms", "Craft the Servo arms Mk II", false));
            Beat(8, "deep_hold", 1200, BeatTrigger.CraftCompleted, "rcp_aug_arms_mk2", false, "The deep hold",
                "Optional: hold facing 3 for 10 minutes and the belt strips the reactor housing.", "The belt; radiation",
                ("obj_deep_hold", "Reach the deep hold", true));
            Beat(9, "spine", 900, BeatTrigger.TimerReached, "900", false, "The spine is creaking",
                "If the spine goes, the only way out is the armoury wall, which the belt opens at facing 1.", "Consequences of waiting",
                ("obj_leave", "Get back to the Nellie", false));
            Beat(10, "ledger", 300, BeatTrigger.PreviousBeatCompleted, null, true, "The ledger",
                "Doramin, the Lane Authority, or nobody.", "A choice with consequences",
                ("obj_ledger_choice", "Decide what to do with the ledger", false));
            Beat(11, "outpost", 120, BeatTrigger.PreviousBeatCompleted, null, false, "An outpost, and a name",
                "The fabricator stays aboard while the belt allows. And someone at Patusan is asking about live colonists. Pod 7 on the Torrens: Lin Adair, 9.", "Outposts; the next goal",
                ("obj_mark_outpost", "Mark the Tremolino as an outpost", false));

            var mission = make.Def<MissionScriptDefinition>("mission_tremolino", Folder, "The Tremolino",
                "Find the T2 fabricator on a hulk tumbling through sunlight and an asteroid belt.", "mission", "tremolino");
            mission.countdownSeconds = 3000f; // Par time, not a fail timer.
            mission.countdownIsFailTimer = false;
            mission.pauseCountdownDuringPrompts = true;
            mission.beats = beats.ToArray();

            var turn = make.Def<CountermeasureCardDefinition>("card_turn_to_sun", Folder, "Turn to Sun",
                "From a powered attitude console: turn the hulk one facing. Rooms cool and bake over the turn; the belt grinds a new face.", "card", "countermeasure");
            turn.effect = CountermeasureEffect.TurnHulk;
            turn.powerCost = 3;
            turn.cooldownSeconds = 90f;
            turn.durationSeconds = c.Sun.TurnSeconds;
            turn.noise = 9f;
            c.TurnToSunCard = turn;

            LoreLogDefinition Log(string id, string title, string room, string text)
            {
                var log = make.Def<LoreLogDefinition>(id, Folder, title, title, "log", "tremolino");
                log.hulk = "Tremolino";
                log.roomId = room;
                log.text = text;
                return log;
            }

            var logs = new[]
            {
                Log("log_tremolino_cargo", "Cargo note", "room_crew_decks", "'Special cargo in the vault. Keep it cold, keep it quiet, don't open it. Doramin pays on delivery.'"),
                Log("log_tremolino_leak", "The leak", "room_crew_decks", "'Vault seal failed during the tumble. Something's out. Okonkwo went to look. Okonkwo didn't come back.'"),
                Log("log_tremolino_sunward", "Navigator's note", "room_bridge", "'Turn her and the nest bakes. Turn her too long and the spine goes. Somebody pick.'")
            };
            c.Logs = logs;

            var rescue = make.Def<GoalDefinition>("goal_rescue_sleeper", Folder, "The sleeper in pod 7",
                "A child asleep on the Torrens, and someone at Patusan asking about live colonists.", "goal");
            rescue.steps = new[]
            {
                new GoalStep { label = "Know where she is", kind = GoalStepKind.HaveFlag, flag = "torrens_sleeper_logged", hint = "Pod 7 on the Torrens (logged on the second trip)" },
                new GoalStep { label = "Somewhere safe to take her", kind = GoalStepKind.HaveFlag, flag = "sleeper_refuge_found", hint = "Ask at Patusan, or at a Lane Authority station" }
            };
            rescue.payoff = "Mission 4: the first rescue.";
            c.RescueGoal = rescue;

            mission.commanderXp = 150;
            mission.baseAutoSalvageScrip = 400;
            mission.cardsGranted = new[] { turn };
            mission.nextGoal = rescue;
            mission.loreLogs = logs;
            mission.autosaveOnEvents = new[] { "console_turn_1", "console_turn_2", "console_turn_3", "console_turn_0" };
            mission.completionFlags = new[] { "fabricator_t2_access", "tremolino_outpost" };
            c.Mission = mission;
        }
    }
}
