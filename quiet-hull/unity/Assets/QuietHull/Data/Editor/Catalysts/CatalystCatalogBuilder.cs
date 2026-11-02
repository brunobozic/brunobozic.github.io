using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// The 19 Catalysts at Common: the 11 gear and 5 meta Catalysts of the design doc, plus the bench-only
    /// Strip, Temper and Seal (Path of Exile's remove / value-reroll / lock verbs, in our own terms).
    /// </summary>
    public static class CatalystCatalogBuilder
    {
        private const string Folder = "Catalysts";

        /// <summary>Strip, Temper and Seal drop from champions from mission 4, when the bench opens.</summary>
        public const int BenchCatalystsFromMission = 4;

        public static CatalystCatalog Build(DefinitionMaker make)
        {
            if (make == null)
            {
                throw new ArgumentNullException(nameof(make));
            }

            var byKind = new Dictionary<CatalystKind, CatalystDefinition>();
            var all = new List<CatalystDefinition>(19);

            void Add(CatalystKind kind, string name, string effect, int fromMission = 1)
            {
                var id = "catalyst_" + ToSnake(kind.ToString());
                var def = make.Def<CatalystDefinition>(id, Folder, name, effect, "catalyst", CatalystRules.IsMeta(kind) ? "meta" : "gear");
                def.kind = kind;
                def.rarity = Rarity.Common;
                def.effectText = effect;
                def.dropsFromMission = fromMission;
                byKind[kind] = def;
                all.Add(def);
            }

            Add(CatalystKind.Focus, "Focus", "The Reliquary drops an item of a chosen category (weapon, augment, unit, tool).");
            Add(CatalystKind.Attune, "Attune", "25% bias toward one affix family.");
            Add(CatalystKind.Resonance, "Resonance", "The hulk's Condition pulls affixes ×1.5.");
            Add(CatalystKind.Ascension, "Ascension", "+1 grade, certain up to III.");
            Add(CatalystKind.Crown, "Crown", "+5% Relic chance.");
            Add(CatalystKind.Mastercraft, "Mastercraft", "+1 affix. On an Omen card: Deepen (a second handicap; benefit +25%, Threat +2).");
            Add(CatalystKind.Imprint, "Imprint", "One chosen affix at T1–T2 (on the bench: if there is room).");
            Add(CatalystKind.Refine, "Refine", "+1 tier on the best affix. On an Omen card: +1 level.");
            Add(CatalystKind.Socket, "Socket", "50% chance of +1 socket.");
            Add(CatalystKind.Reroll, "Reroll", "Reroll all affixes. On an Omen card: redraw its handicap.");
            Add(CatalystKind.HiveTainted, "Hive-tainted",
                "Drop: +2 grades, major drawback. Owned gear: amalgam infusion, irreversible. An Omen card: corruption, irreversible.");
            Add(CatalystKind.Lodestone, "Lodestone", "The vault holds +1 Common Catalyst.");
            Add(CatalystKind.Prism, "Prism", "20% of Catalysts dropped this run gain +1 rarity.");
            Add(CatalystKind.Augury, "Augury", "The vault holds 1 Common Omen card.");
            Add(CatalystKind.Magnet, "Magnet", "30% of Catalyst drops become a chosen type.");
            Add(CatalystKind.Twin, "Twin", "25% chance the vault drops a second item of the same type, 2 grades lower.");
            Add(CatalystKind.Strip, "Strip", "Removes one random affix from an item you own.", BenchCatalystsFromMission);
            Add(CatalystKind.Temper, "Temper", "Rerolls the numbers within each affix's tier; the affixes stay.", BenchCatalystsFromMission);
            Add(CatalystKind.Seal, "Seal", "Locks up to 2 affixes against the next Reroll or Strip; used up by it.", BenchCatalystsFromMission);

            return new CatalystCatalog { ByKind = byKind, All = all };
        }

        private static string ToSnake(string pascal)
        {
            var chars = new List<char>(pascal.Length + 4);
            for (var i = 0; i < pascal.Length; i++)
            {
                var c = pascal[i];
                if (char.IsUpper(c) && i > 0)
                {
                    chars.Add('_');
                }

                chars.Add(char.ToLowerInvariant(c));
            }

            return new string(chars.ToArray());
        }
    }
}
