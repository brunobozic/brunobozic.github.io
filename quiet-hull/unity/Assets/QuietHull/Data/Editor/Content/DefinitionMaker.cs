using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Gets or creates definitions through an <see cref="IDefinitionFactory"/>, sets their identity
    /// and keeps them in catalog order. Shared by every content builder.
    /// </summary>
    public sealed class DefinitionMaker
    {
        private readonly IDefinitionFactory _factory;
        private readonly List<DefinitionBase> _all = new List<DefinitionBase>(128);

        public DefinitionMaker(IDefinitionFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        public IReadOnlyList<DefinitionBase> All => _all;

        public T Def<T>(string id, string folder, string displayName, string description, params string[] tags) where T : DefinitionBase
        {
            var definition = _factory.GetOrCreate<T>(id, folder);
            definition.id = id;
            definition.displayName = displayName;
            definition.description = description;
            definition.tags = tags ?? Array.Empty<string>();
            _all.Add(definition);
            return definition;
        }

        /// <summary>Writes the catalog with everything made so far.</summary>
        public DefinitionCatalog WriteCatalog(string catalogId)
        {
            var catalog = _factory.GetOrCreateCatalog(catalogId);
            catalog.catalogId = catalogId;
            catalog.definitions = new List<DefinitionBase>(_all);
            catalog.InvalidateLookup();
            return catalog;
        }
    }
}
