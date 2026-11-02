using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Mines (6) and the hidden compartment. Mines are quiet until they go off and loud when they do:
    /// a minefield is also an alarm. The Sapper trade plants them fastest.
    /// </summary>
    public static class OrdnanceCatalogBuilder
    {
        public const string CatalogId = "cat_ordnance";
        private const string Folder = "Ordnance";

        public static OrdnanceCatalog Build(IDefinitionFactory factory, PrologueContent prologue, MaterialDefinition xenoalloyIngot)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (prologue == null) throw new ArgumentNullException(nameof(prologue));
            if (xenoalloyIngot == null) throw new ArgumentNullException(nameof(xenoalloyIngot));

            var make = new DefinitionMaker(factory);
            var c = new OrdnanceCatalog();
            var mines = new List<MineDefinition>(6);
            var recipes = new List<RecipeDefinition>(6);
            var blueprints = new List<BlueprintDefinition>(6);
            var p = prologue;

            MineDefinition Mine(string id, string name, string description, MineTrigger trigger, float triggerRadius, float cone,
                float effectRadius, float damage, float stun, float slow, float slowSeconds, float lure, float lureSeconds,
                float detonationNoise, int cold, bool friendlyFire, int tier, params MaterialCost[] inputs)
            {
                var mine = make.Def<MineDefinition>(id, Folder, name, description, "mine", trigger.ToString().ToLowerInvariant());
                mine.trigger = trigger;
                mine.triggerRadius = triggerRadius;
                mine.coneDegrees = cone;
                mine.armSeconds = 3f;
                mine.plantSeconds = 4f;
                mine.friendlyFire = friendlyFire;
                mine.carrySlots = 1;
                mine.maxStack = 4;
                mine.effectRadius = effectRadius;
                mine.damage = damage;
                mine.stunSeconds = stun;
                mine.slowFraction = slow;
                mine.slowSeconds = slowSeconds;
                mine.lureNoise = lure;
                mine.lureSeconds = lureSeconds;
                mine.detonationNoise = detonationNoise;
                mine.coldIntensity = cold;
                mine.noiseOnUse = 1f; // planting
                mine.heatOnUse = 0f;
                mines.Add(mine);

                var recipe = make.Def<RecipeDefinition>($"rcp_{id}", Folder, name, $"Makes 2 × {name}.", "recipe", "mine");
                recipe.output = mine;
                recipe.outputMark = Mark.MkI;
                recipe.outputCount = 2;
                recipe.inputs = inputs;
                recipe.itemInputs = Array.Empty<ItemStack>();
                recipe.minimumStationTier = tier;
                recipe.craftSeconds = 15f;
                recipe.requiresBlueprint = true;
                recipes.Add(recipe);

                var blueprint = make.Def<BlueprintDefinition>($"bp_{id}", Folder, $"Blueprint: {name}", description, "blueprint", "mine");
                blueprint.unlocks = recipe;
                blueprints.Add(blueprint);
                return mine;
            }

            c.Frag = Mine("itm_mine_frag", "Frag mine", "The basic mine: kills Crawlers and Spitters in 3 m. Loud.",
                MineTrigger.Proximity, 2f, 0f, 3f, 120f, 0f, 0f, 0f, 0f, 0f, 14f, 0, true, 1,
                new MaterialCost(p.Ordnance, 3), new MaterialCost(p.Plating, 1));
            c.Stun = Mine("itm_mine_stun", "Stun mine", "Stuns everything in 4 m for 4 s: long enough to break a Leaper's jump.",
                MineTrigger.Proximity, 2f, 0f, 4f, 0f, 4f, 0f, 0f, 0f, 0f, 8f, 0, true, 1,
                new MaterialCost(p.Circuitry, 2), new MaterialCost(p.Ordnance, 1));
            c.Cryo = Mine("itm_mine_cryo", "Cryo mine", "A burst of coolant: slows by half for 6 s and leaves the spot at Cold 1. Quiet.",
                MineTrigger.Proximity, 2f, 0f, 4f, 10f, 0f, 0.5f, 6f, 0f, 0f, 4f, 1, true, 1,
                new MaterialCost(p.Circuitry, 2), new MaterialCost(xenoalloyIngot, 1));
            c.Lure = Mine("itm_mine_lure", "Lure", "No blast: plays recorded machinery at noise 10 for 20 s. Pulls the swarm where you want it.",
                MineTrigger.Timed, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 10f, 20f, 0f, 0, false, 1,
                new MaterialCost(p.Circuitry, 1), new MaterialCost(p.Scrap, 2));
            c.Shredder = Mine("itm_mine_shredder", "Shredder", "A directional charge: 200 damage in a 60° cone out to 8 m. Kills Leapers. Very loud.",
                MineTrigger.Directional, 6f, 60f, 8f, 200f, 0f, 0f, 0f, 0f, 0f, 16f, 0, true, 2,
                new MaterialCost(p.Ordnance, 4), new MaterialCost(p.Plating, 2), new MaterialCost(xenoalloyIngot, 1));
            c.Bloodhound = Mine("itm_mine_bloodhound", "Bloodhound", "Sniffs for alien acid: only aliens set it off. Safe to walk past.",
                MineTrigger.AcidSense, 2f, 0f, 3f, 140f, 0f, 0f, 0f, 0f, 0f, 14f, 0, false, 2,
                new MaterialCost(p.Ordnance, 3), new MaterialCost(p.Circuitry, 2), new MaterialCost(xenoalloyIngot, 1));

            var compartment = make.Def<ShuttleModuleDefinition>("mod_hidden_compartment", Folder, "Hidden compartment",
                "A shielded void behind the hold. Hides 8 contraband items and one powered-down synth from Lane Authority scans, 9 times in 10.",
                "module", "contraband");
            compartment.moduleSlotsUsed = 1;
            compartment.extraHoldSlots = 0;
            compartment.oversizeLoadCapacity = 0;
            compartment.hiddenItemSlots = 8;
            compartment.hiddenUnitBerths = 1;
            compartment.scanEvasion = 0.9f;
            compartment.legality = Legality.Contraband;
            compartment.priceScrip = 4000;
            c.HiddenCompartment = compartment;

            c.Mines = mines;
            c.MineRecipes = recipes;
            c.MineBlueprints = blueprints;
            c.Catalog = make.WriteCatalog(CatalogId);
            return c;
        }
    }
}
