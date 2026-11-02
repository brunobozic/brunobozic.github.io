using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>The commander's drifting UI theme.</summary>
    public sealed class UiThemeContent
    {
        public UiThemeDefinition CommanderTheme { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }
        public IReadOnlyList<DefinitionBase> All { get; internal set; }
    }
}
