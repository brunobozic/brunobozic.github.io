using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>The forecast decision points of the campaign (see the Theme tab).</summary>
    public sealed class StoryDecisions
    {
        public TremolinoContent Previous { get; internal set; }

        public DecisionPointDefinition Ledger { get; internal set; }
        public DecisionPointDefinition Autodoctor { get; internal set; }
        public DecisionPointDefinition PatusanRelease { get; internal set; }
        public DecisionPointDefinition Cornelius { get; internal set; }
        public DecisionPointDefinition Jones { get; internal set; }
        public DecisionPointDefinition SplicesAtTransfer { get; internal set; }
        public DecisionPointDefinition FirstFieldTrial { get; internal set; }

        /// <summary>The finale: the one option the company has no number for.</summary>
        public DecisionPointDefinition Finale { get; internal set; }

        public IReadOnlyList<DecisionPointDefinition> AllDecisions { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
