using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Handles to everything mission 3 (the Tremolino) adds.</summary>
    public sealed class TremolinoContent
    {
        public ColdSleepContent Previous { get; internal set; }
        public FactionCatalog Factions => Previous.Factions;

        public AlienDefinition Husk { get; internal set; }
        public AlienDefinition Bloat { get; internal set; }

        public SunExposureDefinition Sun { get; internal set; }
        public BeltErosionDefinition Belt { get; internal set; }
        public HiveClockDefinition Hive { get; internal set; }
        public HazardZoneDefinition DeepHoldRadiation { get; internal set; }

        public KeyItemDefinition RegulatorCore { get; internal set; }
        public WorkstationDefinition T2Fabricator { get; internal set; }
        public OutpostDefinition Outpost { get; internal set; }

        public LootCacheDefinition Armoury { get; internal set; }
        public LootCacheDefinition PartsStore { get; internal set; }
        public LootCacheDefinition CryoVault { get; internal set; }
        public LootCacheDefinition DeepHold { get; internal set; }

        public OmenCardDefinition SolarFlare { get; internal set; }
        public OmenCardDefinition QuietDecks { get; internal set; }
        public IReadOnlyList<OmenCardDefinition> OmenRewards { get; internal set; }

        public StoryChoiceDefinition Ledger { get; internal set; }
        public MarketOfferDefinition Rumour { get; internal set; }
        public MarketOfferDefinition RadiationLining { get; internal set; }

        public LootCacheDefinition SecuritySynthRemains { get; internal set; }
        public CountermeasureCardDefinition TurnToSunCard { get; internal set; }
        public GoalDefinition RescueGoal { get; internal set; }
        public IReadOnlyList<LoreLogDefinition> Logs { get; internal set; }
        public MissionScriptDefinition Mission { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
