using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>The Common Catalysts: one per kind. Higher rarities follow the "Catalysts by rarity" table.</summary>
    public sealed class CatalystCatalog
    {
        public IReadOnlyDictionary<CatalystKind, CatalystDefinition> ByKind { get; internal set; }
        public IReadOnlyList<CatalystDefinition> All { get; internal set; }

        public CatalystDefinition this[CatalystKind kind] => ByKind[kind];
    }
}
