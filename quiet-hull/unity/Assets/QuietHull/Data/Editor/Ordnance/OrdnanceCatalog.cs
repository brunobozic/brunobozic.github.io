using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Mines, their recipes and blueprints, and the hidden compartment module.</summary>
    public sealed class OrdnanceCatalog
    {
        public MineDefinition Frag { get; internal set; }
        public MineDefinition Stun { get; internal set; }
        public MineDefinition Cryo { get; internal set; }
        public MineDefinition Lure { get; internal set; }
        public MineDefinition Shredder { get; internal set; }
        public MineDefinition Bloodhound { get; internal set; }
        public IReadOnlyList<MineDefinition> Mines { get; internal set; }
        public IReadOnlyList<RecipeDefinition> MineRecipes { get; internal set; }
        public IReadOnlyList<BlueprintDefinition> MineBlueprints { get; internal set; }
        public ShuttleModuleDefinition HiddenCompartment { get; internal set; }
        public DefinitionCatalog Catalog { get; internal set; }
    }
}
