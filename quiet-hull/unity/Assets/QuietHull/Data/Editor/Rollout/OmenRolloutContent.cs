using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Handles to the tarot rollout for missions 1–10 and the Omen cards it adds after mission 3.</summary>
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

        /// <summary>The cards this pack adds (missions 4–10).</summary>
        public IReadOnlyList<OmenCardDefinition> NewCards { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
