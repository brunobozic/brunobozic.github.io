using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Typed handles to everything the prologue builder produced.</summary>
    public sealed class PrologueContent
    {
        // Materials
        public MaterialDefinition Scrap { get; internal set; }
        public MaterialDefinition Plating { get; internal set; }
        public MaterialDefinition Circuitry { get; internal set; }
        public MaterialDefinition Ordnance { get; internal set; }

        // Weapons, tools, throwables
        public MeleeWeaponDefinition ShockProd { get; internal set; }
        public RangedWeaponDefinition ServicePistol { get; internal set; }
        public ThrowableDefinition StunGrenade { get; internal set; }
        public ToolDefinition ArcWelder { get; internal set; }
        public ToolDefinition Disassembler { get; internal set; }

        // Hauling
        public CartDefinition HandCart { get; internal set; }
        public CargoDefinition FuelCylinder { get; internal set; }
        public CargoDefinition CryoCell { get; internal set; }
        public ResourceDefinition GravLatticeAlloy { get; internal set; }
        public KeyItemDefinition ValveKeycard { get; internal set; }

        // Deployables
        public SentryDefinition SentryMk1 { get; internal set; }
        public AmmunitionDefinition SentryAmmoBox { get; internal set; }

        // Crafting
        public RecipeDefinition HandCartRecipe { get; internal set; }
        public RecipeDefinition StunGrenadeRecipe { get; internal set; }
        public RecipeDefinition SentryRecipe { get; internal set; }
        public RecipeDefinition AmmoBoxRecipe { get; internal set; }
        public BlueprintDefinition StunGrenadeBlueprint { get; internal set; }
        public WorkstationDefinition MaintenanceStation { get; internal set; }
        public WorkstationDefinition GrenadeBench { get; internal set; }
        public WorkstationDefinition BayFabricator { get; internal set; }

        // Units
        public UnitDefinition J1M { get; internal set; }
        public UnitDefinition Marlow { get; internal set; }
        public AlienDefinition Crawler { get; internal set; }
        public AlienDefinition Spitter { get; internal set; }

        // World
        public RoomAmbienceDefinition CrusherBay { get; internal set; }
        public RoomAmbienceDefinition PumpHall { get; internal set; }
        public RoomAmbienceDefinition ValveGallery { get; internal set; }
        public LootCacheDefinition MaintenanceShelves { get; internal set; }
        public LootCacheDefinition SecurityOfficerCorpse { get; internal set; }
        public LootCacheDefinition QuartermasterCage { get; internal set; }
        public LootCacheDefinition ChessEngineer { get; internal set; }
        public HazardZoneDefinition DarkReactorCorridor { get; internal set; }
        public HazardZoneDefinition CoolantFlood { get; internal set; }

        // Script
        public IReadOnlyList<WaveDefinition> StreamWaves { get; internal set; }
        public WaveDefinition StillCrawlers { get; internal set; }
        public WaveDefinition ThawingCrawlers { get; internal set; }
        public AnnouncementScheduleDefinition Warden { get; internal set; }
        public MissionScriptDefinition Mission { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }

        /// <summary>Every definition, in catalog order.</summary>
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
