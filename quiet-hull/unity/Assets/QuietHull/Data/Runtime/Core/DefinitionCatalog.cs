using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A set of definitions with a fast id lookup. One catalog per content pack
    /// (the prologue has its own). The lookup is built lazily and published with a
    /// single volatile write, so concurrent readers (main thread, sim thread, jobs
    /// setup) never see a half-built dictionary and never take a lock after warm-up.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Definition Catalog", fileName = "DefinitionCatalog")]
    public sealed class DefinitionCatalog : ScriptableObject
    {
        [SerializeField] internal string catalogId = string.Empty;
        [SerializeField] internal List<DefinitionBase> definitions = new List<DefinitionBase>();

        private readonly object _buildLock = new object();
        private Dictionary<string, DefinitionBase> _byId;

        public string CatalogId => catalogId;
        public IReadOnlyList<DefinitionBase> Definitions => definitions;

        public bool TryGet<T>(string definitionId, out T definition) where T : DefinitionBase
        {
            definition = null;
            if (string.IsNullOrEmpty(definitionId))
            {
                return false;
            }

            if (Lookup().TryGetValue(definitionId, out var found) && found is T typed)
            {
                definition = typed;
                return true;
            }

            return false;
        }

        public T Get<T>(string definitionId) where T : DefinitionBase
        {
            if (TryGet<T>(definitionId, out var definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Catalog '{catalogId}' has no {typeof(T).Name} with id '{definitionId}'.");
        }

        /// <summary>Returns every definition of type <typeparamref name="T"/>, in catalog order.</summary>
        public IEnumerable<T> All<T>() where T : DefinitionBase
        {
            foreach (var definition in definitions)
            {
                if (definition is T typed)
                {
                    yield return typed;
                }
            }
        }

        /// <summary>Validates every definition plus catalog-level rules (unique ids, no missing entries).</summary>
        public ValidationReport Validate()
        {
            var report = new ValidationReport();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    report.Error(null, $"{nameof(definitions)}[{i}]", $"Catalog '{catalogId}' has an empty slot.");
                    continue;
                }

                if (!seen.Add(definition.Id))
                {
                    report.Error(definition, "id", $"is duplicated in catalog '{catalogId}'.");
                }

                definition.Validate(report);
            }

            return report;
        }

        /// <summary>Drops the cached lookup; call after editing the definition list.</summary>
        public void InvalidateLookup() => Volatile.Write(ref _byId, null);

        private Dictionary<string, DefinitionBase> Lookup()
        {
            var lookup = Volatile.Read(ref _byId);
            if (lookup != null)
            {
                return lookup;
            }

            lock (_buildLock)
            {
                lookup = _byId;
                if (lookup != null)
                {
                    return lookup;
                }

                lookup = new Dictionary<string, DefinitionBase>(definitions.Count, StringComparer.Ordinal);
                foreach (var definition in definitions)
                {
                    // First one wins; duplicates are reported by Validate().
                    // ReferenceEquals, not Unity's overloaded ==, which may touch native state off the main thread.
                    if (!ReferenceEquals(definition, null) && !string.IsNullOrEmpty(definition.Id) && !lookup.ContainsKey(definition.Id))
                    {
                        lookup.Add(definition.Id, definition);
                    }
                }

                Volatile.Write(ref _byId, lookup);
                return lookup;
            }
        }

        private void OnEnable() => InvalidateLookup();

        private void OnValidate() => InvalidateLookup();
    }
}
