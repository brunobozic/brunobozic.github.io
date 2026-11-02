using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Every upgrade family: 32 synth body augments (8 slots × 4 lines, Mk I–VIII) and 18 worker
    /// modules (Frame, Hardening, Maintenance, Tool mount; Mk I–XII), the unit mark curves, and the
    /// black-market Shade infiltration frame. Values are Mk I; <see cref="AugmentMath"/> scales them.
    /// Military parts upgrade a synth in military ways; worker modules make better workers. Either kind
    /// can be built fully loud (assault, heavy loader) or fully stealthy (shadow).
    /// </summary>
    public static class AugmentCatalogBuilder
    {
        public const string CatalogId = "cat_augments";

        private const string SynthFolder = "Augments/Synth";
        private const string WorkerFolder = "Augments/Worker";
        private const string UnitsFolder = "Augments/Units";

        public static AugmentCatalog Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var make = new DefinitionMaker(factory);
            var catalog = new AugmentCatalog();

            BuildCurves(catalog, make);
            BuildSynthAugments(catalog, make);
            BuildWorkerModules(catalog, make);
            BuildWorkerTrades(catalog, make);
            BuildShade(catalog, make);

            catalog.Catalog = make.WriteCatalog(CatalogId);
            return catalog;
        }

        private static void BuildCurves(AugmentCatalog c, DefinitionMaker make)
        {
            var worker = make.Def<UnitMarkCurveDefinition>("curve_worker", UnitsFolder, "Worker android marks",
                "Mk I–XII. Older marks run Gen 1 firmware and glitch: their asset directive counts alien eggs and the Queen as company property.", "curve");
            worker.maxMark = Mark.MkXII;
            worker.healthPerMark = 0.15f;
            worker.armourPerMark = 3f;
            worker.moveSpeedPerMark = 0.05f;
            worker.workSpeedPerMark = 0.06f;
            worker.defendDamagePerMark = 0.1f;
            worker.noisePerMark = -0.1f;
            worker.minimumNoise = 1f;
            worker.powerOutputPerMark = 15f;
            worker.extraCarrySlotEvery = 4;
            worker.priceFactorPerMark = 1.35f;
            worker.hasFirmware = true;
            worker.gen2FromMark = Mark.MkV;
            worker.gen3FromMark = Mark.MkIX;
            worker.gen1GlitchChance = 0.25f;
            worker.gen2GlitchChance = 0.10f;
            c.WorkerCurve = worker;

            var military = make.Def<UnitMarkCurveDefinition>("curve_military", UnitsFolder, "Military chassis marks",
                "Mk I–V chassis. Augments carry most of a military unit's growth.", "curve");
            military.maxMark = Mark.MkV;
            military.healthPerMark = 0.12f;
            military.armourPerMark = 4f;
            military.moveSpeedPerMark = 0.05f;
            military.workSpeedPerMark = 0.03f;
            military.defendDamagePerMark = 0f;
            military.noisePerMark = 0f;
            military.minimumNoise = 0.5f;
            military.powerOutputPerMark = 40f;
            military.extraCarrySlotEvery = 0;
            military.priceFactorPerMark = 1.6f;
            military.hasFirmware = false;
            military.gen2FromMark = Mark.MkII;
            military.gen3FromMark = Mark.MkIII;
            military.gen1GlitchChance = 0f;
            military.gen2GlitchChance = 0f;
            c.MilitaryCurve = military;
        }

        private static Legality LegalityOf(AugmentLine line) =>
            line == AugmentLine.Assault ? Legality.Restricted : line == AugmentLine.Shadow ? Legality.Contraband : Legality.Legal;

        private static AugmentDefinition Family(DefinitionMaker make, string folder, string id, string name, string description,
            AugmentSlot slot, AugmentLine line, Legality legality, Mark maxMark,
            float weightKg, float drawW, float outputW, float noise, float heat, float vibration, float aggro, int price,
            params StatModifier[] modifiers)
        {
            var family = make.Def<AugmentDefinition>(id, folder, name, description, "augment", line.ToString().ToLowerInvariant(), slot.ToString().ToLowerInvariant(), id);
            family.slot = slot;
            family.kind = AugmentKind.Cyber;
            family.line = line;
            family.legality = legality;
            family.maxMark = maxMark;
            family.mark = Mark.MkI;
            family.rarity = Rarity.Common;
            family.weightKg = weightKg;
            family.powerDrawWatts = drawW;
            family.powerOutputWatts = outputW;
            family.noise = noise;
            family.heat = heat;
            family.vibration = vibration;
            family.aggro = aggro;
            family.priceScrip = price;
            family.modifiers = modifiers;
            return family;
        }

        private static void BuildSynthAugments(AugmentCatalog c, DefinitionMaker make)
        {
            void S(AugmentSlot slot, AugmentLine line, string name, string description,
                float kg, float draw, float output, float noise, float heat, float vibration, float aggro, int price, params StatModifier[] mods)
            {
                var id = $"aug_{slot.ToString().ToLowerInvariant()}_{line.ToString().ToLowerInvariant()}";
                c.AddSynth(Family(make, SynthFolder, id, name, description, slot, line, LegalityOf(line), Mark.MkVIII,
                    kg, draw, output, noise, heat, vibration, aggro, price, mods));
            }

            // Brain
            S(AugmentSlot.Brain, AugmentLine.Assault, "Kill-switch cortex", "Combat reflexes: acquires and commits faster.",
                2f, 60f, 0f, 0f, 1f, 0f, 0.05f, 600,
                StatModifier.Percent(UnitStat.TargetAcquisitionTime, -0.30f), StatModifier.Add(UnitStat.CritChance, 0.05f));
            S(AugmentSlot.Brain, AugmentLine.Standard, "Tactical cortex", "Learns faster, reacts faster, resists EMP and hacking.",
                2f, 40f, 0f, 0f, 0f, 0f, 0f, 450,
                StatModifier.Percent(UnitStat.ExperienceGain, 0.20f), StatModifier.Percent(UnitStat.TargetAcquisitionTime, -0.30f), StatModifier.Add(UnitStat.EmpResistance, 1f));
            S(AugmentSlot.Brain, AugmentLine.Fine, "Analyst cortex", "A scholar's mind: learns fastest, recovers abilities sooner.",
                1f, 25f, 0f, 0f, -0.5f, 0f, 0f, 500,
                StatModifier.Percent(UnitStat.ExperienceGain, 0.30f), StatModifier.Percent(UnitStat.AbilityCooldown, -0.10f));
            S(AugmentSlot.Brain, AugmentLine.Shadow, "Ghost cortex", "Infiltration firmware: holds perfectly still when watched.",
                1f, 30f, 0f, 0f, -1f, 0f, -0.15f, 2000,
                StatModifier.Add(UnitStat.Visibility, -0.15f), StatModifier.Percent(UnitStat.ExperienceGain, 0.10f));

            // Eyes
            S(AugmentSlot.Eyes, AugmentLine.Assault, "Targeting optics", "A laser designator: finds weak points, and glints.",
                1.5f, 50f, 0f, 0f, 0.5f, 0f, 0.05f, 600,
                StatModifier.Add(UnitStat.CritChance, 0.08f), StatModifier.Add(UnitStat.Accuracy, 0.05f), StatModifier.Add(UnitStat.Visibility, 0.10f));
            S(AugmentSlot.Eyes, AugmentLine.Standard, "Optic array", "Sees further and through smoke; finds weak points.",
                1f, 30f, 0f, 0f, 0f, 0f, 0f, 450,
                StatModifier.Add(UnitStat.VisionRange, 4f), StatModifier.Add(UnitStat.CritChance, 0.05f), StatModifier.Add(UnitStat.SeesThroughSmoke, 1f));
            S(AugmentSlot.Eyes, AugmentLine.Fine, "Thermal optics", "Sees heat: aliens behind smoke, in vents, in the dark.",
                1f, 25f, 0f, 0f, 0f, 0f, 0f, 500,
                StatModifier.Add(UnitStat.VisionRange, 2f), StatModifier.Add(UnitStat.ThermalVision, 1f));
            S(AugmentSlot.Eyes, AugmentLine.Shadow, "Passive optics", "Sees in the dark without a lamp; nothing to give it away.",
                0.5f, 15f, 0f, 0f, 0f, 0f, 0f, 1800,
                StatModifier.Add(UnitStat.DarkVision, 8f), StatModifier.Add(UnitStat.Visibility, -0.10f));

            // Ears
            S(AugmentSlot.Ears, AugmentLine.Assault, "Battle audio", "Active pinging: long-range contacts, and the hive hears the pings.",
                1f, 40f, 0f, 0.5f, 0f, 0f, 0.05f, 550,
                StatModifier.Add(UnitStat.ContactDetectionRange, 15f), StatModifier.Add(UnitStat.Accuracy, 0.03f));
            S(AugmentSlot.Ears, AugmentLine.Standard, "Audio sensors", "Hears through one more door; puts close contacts on the tracker.",
                1f, 25f, 0f, 0f, 0f, 0f, 0f, 450,
                StatModifier.Add(UnitStat.DoorsHeardThrough, 1f), StatModifier.Add(UnitStat.ContactDetectionRange, 12f));
            S(AugmentSlot.Ears, AugmentLine.Fine, "Echo mapper", "Maps by passive sound: longer range, hears into the ducts.",
                0.5f, 20f, 0f, 0f, 0f, 0f, 0f, 500,
                StatModifier.Add(UnitStat.ContactDetectionRange, 16f), StatModifier.Add(UnitStat.DoorsHeardThrough, 1f));
            S(AugmentSlot.Ears, AugmentLine.Shadow, "Noise canceller", "Emits anti-phase sound: footsteps and actions go quiet.",
                1f, 50f, 0f, -2f, 0f, 0f, 0f, 2200,
                StatModifier.Percent(UnitStat.ActionNoise, -0.20f));

            // Lungs (cooling)
            S(AugmentSlot.Lungs, AugmentLine.Assault, "Overclock radiator", "Dumps heat so everything can run hot.",
                4f, 20f, 0f, 0.5f, 2f, 0f, 0.05f, 650,
                StatModifier.Percent(UnitStat.FireRate, 0.10f), StatModifier.Add(UnitStat.HeatShielding, 1f));
            S(AugmentSlot.Lungs, AugmentLine.Standard, "Heat exchanger", "Shrugs off heat and cold; cools down faster.",
                3f, 20f, 0f, 0f, 0f, 0f, 0f, 450,
                StatModifier.Add(UnitStat.HeatShielding, 1f), StatModifier.Add(UnitStat.ColdShielding, 1f), StatModifier.Add(UnitStat.HeatAfterExertion, -2f));
            S(AugmentSlot.Lungs, AugmentLine.Fine, "Coolant loop", "Runs cool even after a fight.",
                2f, 15f, 0f, 0f, -1f, 0f, 0f, 500,
                StatModifier.Add(UnitStat.HeatAfterExertion, -2f), StatModifier.Add(UnitStat.ColdShielding, 1f));
            S(AugmentSlot.Lungs, AugmentLine.Shadow, "Cold sink", "Bleeds heat into a cryogenic sink: invisible to heat vision.",
                2f, 35f, 0f, 0f, -2f, 0f, 0f, 2400,
                StatModifier.Add(UnitStat.HeatAfterExertion, -3f));

            // Heart (power core)
            S(AugmentSlot.Heart, AugmentLine.Assault, "Assault reactor", "Huge output. Hums, runs hot, and the hive can feel it.",
                8f, 0f, 600f, 1f, 2f, 0.5f, 0.10f, 700,
                StatModifier.Add(UnitStat.HealthRegenOutOfCombat, 2f));
            S(AugmentSlot.Heart, AugmentLine.Standard, "Micro-reactor", "Repairs itself out of combat; abilities come back sooner.",
                5f, 0f, 400f, 0.5f, 1f, 0f, 0f, 450,
                StatModifier.Add(UnitStat.HealthRegenOutOfCombat, 2f), StatModifier.Percent(UnitStat.AbilityCooldown, -0.15f));
            S(AugmentSlot.Heart, AugmentLine.Fine, "Fuel cell", "Quiet, cool, modest output.",
                3f, 0f, 300f, 0f, 0.5f, 0f, 0f, 500,
                StatModifier.Percent(UnitStat.AbilityCooldown, -0.05f));
            S(AugmentSlot.Heart, AugmentLine.Shadow, "Shadow cell", "A silent, cold cell. Nobody admits to making them.",
                3f, 0f, 350f, 0f, -0.5f, 0f, 0f, 2500,
                StatModifier.Add(UnitStat.HealthRegenOutOfCombat, 1f));

            // Arms
            S(AugmentSlot.Arms, AugmentLine.Assault, "Heavy servos", "Soaks recoil, hits like a door, carries heavy weapons.",
                10f, 120f, 0f, 0.5f, 1f, 0f, 0.10f, 700,
                StatModifier.Percent(UnitStat.FireRate, 0.10f), StatModifier.Add(UnitStat.Accuracy, 0.10f),
                StatModifier.Percent(UnitStat.MeleeDamage, 0.40f), StatModifier.Add(UnitStat.HeavyWeaponHandling, 1f));
            S(AugmentSlot.Arms, AugmentLine.Standard, "Servo arms", "Faster hands: more damage from any weapon.",
                6f, 60f, 0f, 0f, 0f, 0f, 0f, 450,
                StatModifier.Percent(UnitStat.FireRate, 0.15f), StatModifier.Percent(UnitStat.ReloadTime, -0.25f), StatModifier.Percent(UnitStat.MeleeDamage, 0.20f));
            S(AugmentSlot.Arms, AugmentLine.Fine, "Precision manipulators", "Steady hands: accuracy, fast reloads, clean hits.",
                3f, 35f, 0f, 0f, 0f, 0f, 0f, 500,
                StatModifier.Add(UnitStat.Accuracy, 0.08f), StatModifier.Percent(UnitStat.ReloadTime, -0.30f), StatModifier.Add(UnitStat.CritChance, 0.03f));
            S(AugmentSlot.Arms, AugmentLine.Shadow, "Silent actuators", "Reloads, strikes and work make almost no sound.",
                3f, 40f, 0f, 0f, 0f, 0f, 0f, 2000,
                StatModifier.Percent(UnitStat.ActionNoise, -0.30f), StatModifier.Percent(UnitStat.MeleeDamage, 0.10f));

            // Legs
            S(AugmentSlot.Legs, AugmentLine.Assault, "Hydraulic legs", "Fast and strong; every step shakes the deck.",
                14f, 100f, 0f, 1.5f, 0f, 2f, 0.10f, 700,
                StatModifier.Percent(UnitStat.MoveSpeed, 0.25f), StatModifier.Add(UnitStat.CarryCapacityKg, 20f));
            S(AugmentSlot.Legs, AugmentLine.Standard, "Damped legs", "Faster and quieter.",
                8f, 50f, 0f, 0f, 0f, 0f, 0f, 450,
                StatModifier.Percent(UnitStat.MoveSpeed, 0.20f), StatModifier.Add(UnitStat.NoiseWalking, -1f));
            S(AugmentSlot.Legs, AugmentLine.Fine, "Padded actuators", "Soft feet: quiet, a little faster.",
                5f, 30f, 0f, -1.5f, 0f, -1f, 0f, 500,
                StatModifier.Percent(UnitStat.MoveSpeed, 0.10f));
            S(AugmentSlot.Legs, AugmentLine.Shadow, "Shadow gait", "Silent, no vibration.",
                4f, 40f, 0f, -2.5f, 0f, -2f, 0f, 2600,
                StatModifier.Percent(UnitStat.MoveSpeed, 0.05f));

            // Dermis
            S(AugmentSlot.Dermis, AugmentLine.Assault, "Plate dermis", "Heavy armour plate. Clanks.",
                25f, 20f, 0f, 1f, 0.5f, 0f, 0.15f, 750,
                StatModifier.Add(UnitStat.Armour, 35f), StatModifier.Percent(UnitStat.MaxHealth, 0.25f));
            S(AugmentSlot.Dermis, AugmentLine.Standard, "Ablative dermis", "Armoured and thermally damped.",
                10f, 10f, 0f, 0f, -1f, 0f, 0f, 450,
                StatModifier.Add(UnitStat.Armour, 15f));
            S(AugmentSlot.Dermis, AugmentLine.Fine, "Thermal mesh", "Light armour that hides heat.",
                4f, 15f, 0f, 0f, -1.5f, 0f, 0f, 500,
                StatModifier.Add(UnitStat.Armour, 5f));
            S(AugmentSlot.Dermis, AugmentLine.Shadow, "Shadow skin", "Light-bending, heat-damping skin: eyes slide off it.",
                5f, 45f, 0f, 0f, -1f, 0f, 0f, 3000,
                StatModifier.Add(UnitStat.Visibility, -0.30f), StatModifier.Add(UnitStat.Armour, 5f));
        }

        private static void BuildWorkerModules(AugmentCatalog c, DefinitionMaker make)
        {
            var modules = new List<AugmentDefinition>(18);
            var hardening = new List<AugmentDefinition>(6);

            AugmentDefinition W(string id, string name, string description, AugmentSlot slot, AugmentLine line, Legality legality,
                float kg, float draw, float noise, float heat, float vibration, float aggro, int price, params StatModifier[] mods)
            {
                var module = Family(make, WorkerFolder, id, name, description, slot, line, legality, Mark.MkXII,
                    kg, draw, 0f, noise, heat, vibration, aggro, price, mods);
                modules.Add(module);
                return module;
            }

            // Frame: what kind of worker it is.
            c.LoaderFrame = W("wmod_frame_loader", "Loader frame", "A heavy loader: carries crates without a cart and hits like a forklift. Not subtle.",
                AugmentSlot.Frame, AugmentLine.Assault, Legality.Legal, 40f, 150f, 3f, 1.5f, 3f, 0.15f, 600,
                StatModifier.Add(UnitStat.CarrySlots, 3f), StatModifier.Percent(UnitStat.MeleeDamage, 1.00f),
                StatModifier.Percent(UnitStat.MaxHealth, 0.40f), StatModifier.Add(UnitStat.Armour, 10f));
            c.ReinforcedFrame = W("wmod_frame_reinforced", "Reinforced frame", "More health and armour for hard sites.",
                AugmentSlot.Frame, AugmentLine.Standard, Legality.Legal, 15f, 20f, 0.5f, 0f, 0f, 0f, 350,
                StatModifier.Percent(UnitStat.MaxHealth, 0.25f), StatModifier.Add(UnitStat.Armour, 8f));
            W("wmod_frame_light", "Light frame", "Lighter and quicker; quieter on its feet.",
                AugmentSlot.Frame, AugmentLine.Fine, Legality.Legal, 6f, 10f, -1f, 0f, -0.5f, 0f, 350,
                StatModifier.Percent(UnitStat.MoveSpeed, 0.10f), StatModifier.Percent(UnitStat.WorkSpeed, 0.10f));
            c.MuffledFrame = W("wmod_frame_muffled", "Muffled frame", "A smuggler's worker: padded, damped, hard to see.",
                AugmentSlot.Frame, AugmentLine.Shadow, Legality.Contraband, 8f, 30f, -2f, -0.5f, -2f, -0.10f, 1800,
                StatModifier.Add(UnitStat.Visibility, -0.20f));

            // Hardening: two sockets; one kit per hulk condition (no-G, radiation, acid, heat, cold, EMP).
            AugmentDefinition H(string id, string name, string description, UnitStat shielding)
            {
                var module = W(id, name, description, AugmentSlot.Hardening, AugmentLine.Standard, Legality.Legal,
                    4f, 5f, 0f, 0f, 0f, 0f, 250, StatModifier.Add(shielding, 1f));
                hardening.Add(module);
                return module;
            }

            H("wmod_hard_maglock", "Mag-lock boots", "Works in zero-G and gravity shear.", UnitStat.GravityShielding);
            H("wmod_hard_rad", "Radiation lining", "Works in reactor spaces.", UnitStat.RadiationShielding);
            H("wmod_hard_acid", "Acid-proof coating", "Shrugs off Bloat and Spitter acid.", UnitStat.AcidShielding);
            H("wmod_hard_heat", "Heat jacket", "Works in burning and sunward rooms.", UnitStat.HeatShielding);
            H("wmod_hard_cold", "Cold jacket", "Works in frozen and flooded rooms.", UnitStat.ColdShielding);
            H("wmod_hard_emp", "EMP cage", "Keeps working through ion storms and EMP.", UnitStat.IonShielding);

            // Maintenance: keeping itself (and others) running.
            W("wmod_maint_patch", "Patch welder", "Welds its own wounds mid-fight. Loud and hot.",
                AugmentSlot.Maintenance, AugmentLine.Assault, Legality.Legal, 6f, 60f, 1f, 1f, 0f, 0.05f, 500,
                StatModifier.Add(UnitStat.HealthRegenPerSecond, 3f));
            c.SelfRepair = W("wmod_maint_nanites", "Self-repair nanites", "Repairs itself, slowly in a fight, faster out of one.",
                AugmentSlot.Maintenance, AugmentLine.Standard, Legality.Legal, 2f, 30f, 0f, 0f, 0f, 0f, 400,
                StatModifier.Add(UnitStat.HealthRegenPerSecond, 1f), StatModifier.Add(UnitStat.HealthRegenOutOfCombat, 3f));
            W("wmod_maint_field_kit", "Field repair kit", "Repairs other units and sentries.",
                AugmentSlot.Maintenance, AugmentLine.Fine, Legality.Legal, 4f, 20f, 0f, 0f, 0f, 0f, 400,
                StatModifier.Add(UnitStat.RepairOthersPerSecond, 15f));
            W("wmod_maint_quiet_nanites", "Quiet nanites", "Silent self-repair for workers who mustn't be heard.",
                AugmentSlot.Maintenance, AugmentLine.Shadow, Legality.Contraband, 2f, 30f, 0f, -0.5f, 0f, 0f, 1500,
                StatModifier.Add(UnitStat.HealthRegenOutOfCombat, 2f), StatModifier.Percent(UnitStat.ActionNoise, -0.20f));

            // Tool mount: how it works.
            c.BreachingCutter = W("wmod_tool_breacher", "Breaching cutter", "Cuts faster and hits hard. Sparks, noise, heat.",
                AugmentSlot.ToolMount, AugmentLine.Assault, Legality.Legal, 8f, 80f, 2f, 2f, 1f, 0.10f, 550,
                StatModifier.Percent(UnitStat.WorkSpeed, 0.40f), StatModifier.Percent(UnitStat.MeleeDamage, 0.60f));
            W("wmod_tool_servos", "Work servos", "Faster at every job.",
                AugmentSlot.ToolMount, AugmentLine.Standard, Legality.Legal, 4f, 40f, 0f, 0f, 0f, 0f, 350,
                StatModifier.Percent(UnitStat.WorkSpeed, 0.20f));
            W("wmod_tool_quiet", "Quiet tools", "Dampened tool heads: slower, much quieter.",
                AugmentSlot.ToolMount, AugmentLine.Fine, Legality.Legal, 3f, 30f, 0f, 0f, 0f, 0f, 400,
                StatModifier.Percent(UnitStat.ActionNoise, -0.40f), StatModifier.Percent(UnitStat.WorkSpeed, 0.05f));
            c.SilentTools = W("wmod_tool_silent", "Silent tools", "Smuggler's tools: almost no sound at all.",
                AugmentSlot.ToolMount, AugmentLine.Shadow, Legality.Contraband, 3f, 40f, 0f, 0f, -0.5f, 0f, 1500,
                StatModifier.Percent(UnitStat.ActionNoise, -0.70f));

            c.MiningBeam = W("wmod_tool_mining_beam", "Mining beam", "Replaces one hand with a high-power beam: mines fast, burns anything close. Hot.",
                AugmentSlot.ToolMount, AugmentLine.Assault, Legality.Legal, 9f, 120f, 1f, 3f, 0f, 0.10f, 650,
                StatModifier.Percent(UnitStat.MiningSpeed, 1.00f), StatModifier.Percent(UnitStat.MeleeDamage, 0.50f));
            c.ConstructionRig = W("wmod_tool_construction_rig", "Construction rig", "Welder, sealer and printer head in one: builds faster on less power.",
                AugmentSlot.ToolMount, AugmentLine.Fine, Legality.Legal, 6f, 50f, 0f, 0.5f, 0f, 0f, 500,
                StatModifier.Percent(UnitStat.ConstructionSpeed, 0.30f), StatModifier.Percent(UnitStat.ToolEnergyCost, -0.20f));

            c.WorkerModules = modules;
            c.HardeningModules = hardening;
        }

        private static void BuildWorkerTrades(AugmentCatalog c, DefinitionMaker make)
        {
            const string folder = "Augments/Trades";

            SpecializationDefinition Trade(string id, string name, string description, Archetype trade, string gateTag, string lockedReason,
                ItemDefinition grants, string abilityId, string abilityName, string ability, float cooldown, float duration,
                string strong, string weak, params StatModifier[] passives)
            {
                var spec = make.Def<SpecializationDefinition>(id, folder, name, description, "specialization", "worker");
                spec.archetype = trade;
                spec.unlockExperience = 40;
                spec.weaponOverride = grants;
                spec.requiresEquipmentTag = gateTag ?? string.Empty;
                spec.lockedReason = lockedReason ?? string.Empty;
                spec.passives = passives;
                spec.abilityId = abilityId;
                spec.abilityName = abilityName;
                spec.abilityDescription = ability;
                spec.abilityCooldownSeconds = cooldown;
                spec.abilityDurationSeconds = duration;
                spec.abilityRadius = 0f;
                spec.strongAgainst = strong;
                spec.weakAgainst = weak;
                return spec;
            }

            c.Hauler = Trade("spec_worker_hauler", "Heavy hauler",
                "Carries a whole cryo bed alone. Every alien on the deck hears it coming: escort it.", Archetype.Hauler,
                "wmod_frame_loader", "Needs a loader frame.", null,
                "ab_brace", "Brace", "For 6 s the hauler can't be knocked down or made to drop its load.", 20f, 6f,
                "Moving oversize loads without a cart or a sled.", "Stealth: carrying an oversize load doubles its aggro and adds noise 4.",
                StatModifier.Add(UnitStat.CarrySlots, 2f), StatModifier.Add(UnitStat.OversizeCarry, 1f), StatModifier.Add(UnitStat.Aggro, 0.5f));

            c.Miner = Trade("spec_worker_miner", "Miner",
                "One hand is replaced by a high-power mining beam.", Archetype.Miner,
                null, null, c.MiningBeam,
                "ab_overcharge_beam", "Overcharge", "For 6 s the beam mines twice as fast and burns anything within 3 m for 25 damage per second.", 25f, 6f,
                "Ore, alloy and amalgam; defending a dig site.", "Heat vision: the beam runs hot.",
                StatModifier.Percent(UnitStat.MiningSpeed, 0.50f), StatModifier.Add(UnitStat.HeatSignature, 1f));

            c.Constructor = Trade("spec_worker_constructor", "Welder-constructor",
                "Welds, seals and builds faster, on less power.", Archetype.Constructor,
                null, null, null,
                "ab_rapid_weld", "Rapid weld", "Seals a door in 2 s.", 30f, 0f,
                "Sealing lines, building sentries and barricades, repairing the shuttle.", "Nothing in a fight; sparks and light give it away.",
                StatModifier.Percent(UnitStat.ConstructionSpeed, 0.40f), StatModifier.Percent(UnitStat.ToolEnergyCost, -0.30f));

            c.Technician = Trade("spec_worker_technician", "Technician",
                "Keeps the sentries and the team running.", Archetype.Technician,
                null, null, null,
                "ab_field_patch", "Field patch", "Repairs a sentry or unit for 40 health per second for 5 s.", 20f, 5f,
                "Sentry lanes: reloads and repairs under fire.", "Fragile; must stand next to what it fixes.",
                StatModifier.Add(UnitStat.RepairOthersPerSecond, 15f), StatModifier.Percent(UnitStat.SentryReloadSpeed, 0.50f));

            c.Sapper = Trade("spec_worker_sapper", "Sapper",
                "A quiet worker that goes first: scouts ahead, lays mines, carries and installs sentries without being heard.", Archetype.Sapper,
                null, null, null,
                "ab_plant", "Plant", "Deploys a carried sentry or mine in 1 s, silently.", 15f, 0f,
                "Scouting; setting up kill zones before the hive wakes; The Ghost and other vows.", "Fighting: it hides, it doesn't hold.",
                StatModifier.Add(UnitStat.NoiseWalking, -1f), StatModifier.Percent(UnitStat.ActionNoise, -0.30f),
                StatModifier.Add(UnitStat.Visibility, -0.15f), StatModifier.Percent(UnitStat.DeploySpeed, 0.50f));
        }

        private static void BuildShade(AugmentCatalog c, DefinitionMaker make)
        {
            var shade = make.Def<UnitDefinition>("unit_shade", UnitsFolder, "Shade infiltration synth",
                "A corporate infiltration frame that doesn't officially exist. Light, quiet, fragile and very expensive. Black market only.",
                "unit", "combat", "synthetic", "contraband");
            shade.maxHealth = 180f;
            shade.armour = 5f;
            shade.canBeDestroyed = true;
            shade.wreckedSeconds = 0f;
            shade.rebootHealthFraction = 0.3f;
            shade.moveSpeed = 3.6f;
            shade.noiseWalking = 1f;
            shade.heatIdle = 1.5f;
            shade.heatAfterExertion = 3f;
            shade.exertionCooldownSeconds = 4f;
            shade.lampRange = 0f;
            shade.handCarrySlots = 0;
            shade.abilities = UnitAbility.Move | UnitAbility.Interact | UnitAbility.Attack | UnitAbility.Patrol | UnitAbility.FireWeapon;
            shade.startingItems = Array.Empty<ItemDefinition>();
            shade.augmentSlots = new[]
            {
                AugmentSlot.Arms, AugmentSlot.Legs, AugmentSlot.Eyes, AugmentSlot.Ears,
                AugmentSlot.Lungs, AugmentSlot.Heart, AugmentSlot.Dermis, AugmentSlot.Brain
            };
            shade.specializationOptions = Array.Empty<SpecializationDefinition>();
            shade.combatRole = CombatRole.Combat;
            shade.defendRadius = 0f;
            shade.defendDamagePerSecond = 0f;
            shade.workSpeed = 1f;
            shade.augmentCapacityKg = 35f;
            shade.basePowerOutputWatts = 250f;
            shade.priceScrip = 12000;
            shade.markCurve = c.MilitaryCurve;
            c.Shade = shade;
        }
    }
}
