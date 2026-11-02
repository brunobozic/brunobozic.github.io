using System;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// The company's forecast for every story decision designed so far. The probabilities are Vashti-Kell's,
    /// not the player's odds: mission 10 prints the player's real choices against them. Values mirror the
    /// "Theme: the guided monster" tab.
    /// </summary>
    public static class StoryDecisionsBuilder
    {
        public const string CatalogId = "cat_story_decisions";

        /// <summary>The mission whose archive shows the file (the Act 2 finale).</summary>
        public const int RevealMission = 10;

        /// <summary>The finale option the company's model can't contain.</summary>
        public const string StayOptionId = "stay";

        private const string Folder = "Story/Decisions";

        public static StoryDecisions Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var previous = TremolinoContentBuilder.Build(factory);
            var make = new DefinitionMaker(factory);
            var d = new StoryDecisions { Previous = previous };

            DecisionPointDefinition Point(string id, string name, string label, int mission, params ForecastOption[] options)
            {
                var point = make.Def<DecisionPointDefinition>(id, Folder, name, $"Forecast decision, mission {mission}.", "decision", "forecast");
                point.fileLabel = label;
                point.mission = mission;
                point.options = options;
                return point;
            }

            ForecastOption O(string id, string label, string line, float p) => new ForecastOption(id, label, line, p);

            d.Ledger = Point("dp_ledger", "The smugglers' ledger", "Ledger", 3,
                O("doramin", "Return it to Doramin", "returned to the broker", 0.52f),
                O("lane", "Sell it to the Lane Authority", "sold to the Lane", 0.27f),
                O("keep", "Keep it", "retained", 0.21f));
            d.Ledger.storyChoice = previous.Ledger;

            d.Autodoctor = Point("dp_m4_autodoctor", "The Torrens's autodoctor", "Pod 7", 4,
                O("take", "Cut it out and haul it home", "medical unit salvaged", 0.81f),
                O("leave", "Leave it", "medical unit abandoned", 0.19f));

            d.PatusanRelease = Point("dp_m6_release", "Where to open the cage", "Patusan", 6,
                O("open", "The station's air system (Open)", "release via ventilation (Open)", 0.67f),
                O("controlled", "The docks' quarantine ring (Controlled)", "release via quarantine ring (Controlled)", 0.33f));

            d.Cornelius = Point("dp_m6_cornelius", "Cornelius", "Cornelius", 6,
                O("spaced", "Space him", "forger spaced", 0.45f),
                O("taken", "Take him", "forger retained as asset", 0.35f),
                O("left", "Leave him", "forger abandoned to the station", 0.20f));

            d.Jones = Point("dp_m7_jones", "Mr. Jones", "Jones", 7,
                O("spaced", "Put him out of an airlock", "spaced", 0.52f),
                O("cryo", "Keep him in cryo", "held in cryo", 0.31f),
                O("sold", "Sell him to the Choir", "sold to the Choir", 0.17f));

            d.SplicesAtTransfer = Point("dp_m7_splices", "Splices at transfer", "Splices at transfer", 7,
                O("0", "None", "0", 0.25f),
                O("1", "One", "1", 0.35f),
                O("2", "Two", "2", 0.30f),
                O("3", "Three", "3", 0.10f));

            d.FirstFieldTrial = Point("dp_m8_field_trial", "The first field trial", "Field trial", 8,
                O("accept", "Install the prototype", "prototype accepted", 0.88f),
                O("refuse", "Refuse it", "prototype refused. Increase pressure", 0.12f));

            // The finale. The model is built on the Patna: this candidate jumps. Every candidate who got this far ran.
            // A candidate who stays aboard has never been observed, so the file has no number for it.
            d.Finale = Point("dp_m18_finale", "The last launch", "Final disposition", 18,
                O("launch", "Launch the Nellie", "candidate launches; collection proceeds", 0.97f),
                O("upriver", "Go upriver to Kurtz", "candidate joins M-4", 0.03f),
                ForecastOption.Unforecast(StayOptionId, "Stay aboard Stein's Light", "candidate stays aboard"));

            d.AllDecisions = new[] { d.Ledger, d.Autodoctor, d.PatusanRelease, d.Cornelius, d.Jones, d.SplicesAtTransfer, d.FirstFieldTrial, d.Finale };
            d.Catalog = make.WriteCatalog(CatalogId);
            d.All = make.All;
            return d;
        }
    }
}
