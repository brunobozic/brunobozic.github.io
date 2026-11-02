using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Typed handles to everything the mission 2 (Cold Sleep) builder produced.</summary>
    public sealed class ColdSleepContent
    {
        public PrologueContent Prologue { get; internal set; }

        // Units
        public UnitDefinition Picket { get; internal set; }
        public UnitDefinition SecondWorker { get; internal set; }
        public AlienDefinition Leaper { get; internal set; }
        public AlienDefinition Constable { get; internal set; }

        // Weapons
        public RangedWeaponDefinition PicketCarbine { get; internal set; }
        public RangedWeaponDefinition LongRifle { get; internal set; }
        public RangedWeaponDefinition FoamProjector { get; internal set; }

        // Growth
        public SpecializationDefinition Marksman { get; internal set; }
        public SpecializationDefinition Controller { get; internal set; }
        public SpecializationDefinition Bulwark { get; internal set; }
        public IReadOnlyList<AugmentDefinition> Augments { get; internal set; }

        // Omens
        public OmenCardDefinition ThinSwarm { get; internal set; }
        public OmenCardDefinition ThickHide { get; internal set; }
        public OmenCardDefinition TheGhost { get; internal set; }

        // Cargo, world, crafting
        public CargoDefinition CryoPod { get; internal set; }
        public CargoDefinition PowerCell { get; internal set; }
        public MaterialDefinition AcousticPanel { get; internal set; }
        public FixtureDefinition PodMount { get; internal set; }
        public RoomTreatmentDefinition AcousticInsulation { get; internal set; }
        public LootCacheDefinition SecurityStore { get; internal set; }
        public LootCacheDefinition BulkheadPanel { get; internal set; }
        public RecipeDefinition SalvageAmmoRecipe { get; internal set; }
        public WorkstationDefinition TorrensFabricator { get; internal set; }

        // Hub
        public ShuttleModuleDefinition CargoCradle { get; internal set; }
        public MarketOfferDefinition NewIdentity { get; internal set; }
        public MarketOfferDefinition TransponderReflash { get; internal set; }
        public MarketOfferDefinition RepairJ1M { get; internal set; }
        public MarketOfferDefinition SecondWorkerOffer { get; internal set; }
        public MarketOfferDefinition PicketOffer { get; internal set; }
        public IReadOnlyList<MarketOfferDefinition> AugmentOffers { get; internal set; }
        public ContractDefinition Contract { get; internal set; }

        // Script
        public WaveDefinition ConstableNoise { get; internal set; }
        public WaveDefinition Trip1Stream { get; internal set; }
        public WaveDefinition Trip2Nest { get; internal set; }
        public WaveDefinition Trip2LeaperIntro { get; internal set; }
        public WaveDefinition Trip2Stream { get; internal set; }
        public MissionScriptDefinition Trip1 { get; internal set; }
        public MissionScriptDefinition Trip2 { get; internal set; }

        public DefinitionCatalog Catalog { get; internal set; }
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
