using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    public sealed class FactionCatalog
    {
        public FactionDefinition HollowMoon { get; internal set; }
        public FactionDefinition LaneAuthority { get; internal set; }
        public FactionDefinition DriftersUnion { get; internal set; }
        public FactionDefinition QuietChoir { get; internal set; }
        public FactionDefinition VashtiKell { get; internal set; }
        public LicenceDefinition LaneMilitaryLicence { get; internal set; }
        public LicenceDefinition IndustrialLicence { get; internal set; }
        public IReadOnlyList<FactionDefinition> All => new[] { HollowMoon, LaneAuthority, DriftersUnion, QuietChoir, VashtiKell };

        /// <summary>A fresh campaign's standing.</summary>
        public PlayerStanding NewStanding() => new PlayerStanding(All);
    }

    public static class FactionCatalogBuilder
    {
        private const string Folder = "Factions";

        public static FactionCatalog Build(DefinitionMaker make)
        {
            var c = new FactionCatalog();

            FactionDefinition F(string id, string name, Standing start, string dealsIn, string description)
            {
                var faction = make.Def<FactionDefinition>(id, Folder, name, description, "faction");
                faction.startingStanding = start;
                faction.dealsIn = dealsIn;
                return faction;
            }

            c.HollowMoon = F("fac_hollow_moon", "Hollow Moon", Standing.Neutral, "Smuggling, fences, contraband, people who need to disappear",
                "Patusan's owners. Courteous, patient and in every dirty trade there is.");
            c.LaneAuthority = F("fac_lane_authority", "Lane Authority", Standing.Neutral, "Licences, lawful salvage, scans at regulated docks",
                "The shipping lanes' police. They can make you legal; they can also find what you're hiding.");
            c.DriftersUnion = F("fac_drifters_union", "Drifters' Union", Standing.Neutral, "Salvage crews, rumours, precision parts, worker modules",
                "Independent salvage crews who look after their own.");
            c.QuietChoir = F("fac_quiet_choir", "Quiet Choir", Standing.Neutral, "Tarot, secrets, the rarest Omen cards",
                "A sect that collects secrets and reads cards. Nobody knows what they want.");
            c.VashtiKell = F("fac_vashti_kell", "Vashti-Kell", Standing.Unfriendly, "The company: colonies, specimens, debt",
                "The company Marlow ran from. They'd like their property back.");

            c.LaneMilitaryLicence = make.Def<LicenceDefinition>("lic_lane_military", Folder, "Lane Authority military licence",
                "Lawful access to military-pattern parts and ordnance; fewer scans.", "licence");
            c.LaneMilitaryLicence.issuedBy = c.LaneAuthority;
            c.LaneMilitaryLicence.scanChanceReduction = 0.5f;

            c.IndustrialLicence = make.Def<LicenceDefinition>("lic_vk_industrial", Folder, "Industrial licence",
                "Vashti-Kell industrial permit for heavy worker frames and breaching tools (forged copies circulate).", "licence");
            c.IndustrialLicence.issuedBy = c.VashtiKell;
            c.IndustrialLicence.scanChanceReduction = 0f;
            return c;
        }
    }
}
