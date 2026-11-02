using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Handles to the tarot rollout for missions 1–10: Major and Minor cards, the Cartomancer and the Catalysts.</summary>
    public sealed class OmenRolloutContent
    {
        public TremolinoContent Previous { get; internal set; }
        public OmenRolloutDefinition Rollout { get; internal set; }

        // Mission 4: Pod 7
        public OmenCardDefinition IronVow { get; internal set; }
        public OmenCardDefinition HungryHive { get; internal set; }

        // Mission 5: the Nan-Shan
        public OmenCardDefinition CullingLeaper { get; internal set; }
        public OmenCardDefinition IonStorm { get; internal set; }

        // Mission 6: the ledger's price
        public OmenCardDefinition BrittleWelds { get; internal set; }
        public OmenCardDefinition TheDeck { get; internal set; }
        public OmenCardDefinition IronChitin { get; internal set; }

        // Mission 7: old patterns
        public OmenCardDefinition EngineeringDeck { get; internal set; }
        public OmenCardDefinition TheFortress { get; internal set; }

        // Mission 8: the Sephora
        public OmenCardDefinition TheWatcher { get; internal set; }
        public OmenCardDefinition LongNight { get; internal set; }

        // Mission 9: second breach
        public OmenCardDefinition SecondBreach { get; internal set; }
        public OmenCardDefinition CullingSpitter { get; internal set; }
        public OmenCardDefinition ChampionTide { get; internal set; }

        // Mission 10: the Ledger
        public OmenCardDefinition BloodPrice { get; internal set; }
        public OmenCardDefinition PrismEclipse { get; internal set; }

        // Minor Arcana (suits); Gleaner's Moon (Cogs) comes from mission 2's pack
        public OmenCardDefinition TheTithe { get; internal set; }
        public OmenCardDefinition BrokersSeal { get; internal set; }
        public OmenCardDefinition TheLamplighter { get; internal set; }
        public OmenCardDefinition TheVeteran { get; internal set; }
        public OmenCardDefinition TheVigil { get; internal set; }

        // Act 3
        public OmenCardDefinition TheAnchorite { get; internal set; }

        // The Cartomancer's table and the Catalysts
        public CartomancerDefinition Cartomancer { get; internal set; }
        public CatalystCatalog Catalysts { get; internal set; }

        /// <summary>The Major cards this pack adds (missions 4–10).</summary>
        public IReadOnlyList<OmenCardDefinition> NewCards { get; internal set; }

        /// <summary>The Minor cards this pack adds (the ledger's three suits, Skulls, and Candles incl. The Deck and Prism Eclipse).</summary>
        public IReadOnlyList<OmenCardDefinition> MinorCards { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
