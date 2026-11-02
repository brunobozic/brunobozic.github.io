using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Supplies the instance a builder configures for a given id. The editor implementation
    /// returns the existing asset when there is one, so regenerating updates assets in place
    /// and every cross-reference points at a persisted object.
    /// </summary>
    public interface IDefinitionFactory
    {
        T GetOrCreate<T>(string id, string folder) where T : DefinitionBase;

        DefinitionCatalog GetOrCreateCatalog(string catalogId);
    }

    /// <summary>In-memory factory for tests and dry runs. Call <see cref="Dispose"/> to free the instances.</summary>
    public sealed class InMemoryDefinitionFactory : IDefinitionFactory, IDisposable
    {
        private readonly Dictionary<string, ScriptableObject> _instances = new Dictionary<string, ScriptableObject>(StringComparer.Ordinal);

        public T GetOrCreate<T>(string id, string folder) where T : DefinitionBase
        {
            if (_instances.TryGetValue(id, out var existing))
            {
                return existing as T ?? throw new InvalidOperationException($"Id '{id}' already exists as {existing.GetType().Name}, not {typeof(T).Name}.");
            }

            var created = ScriptableObject.CreateInstance<T>();
            created.name = id;
            _instances.Add(id, created);
            return created;
        }

        public DefinitionCatalog GetOrCreateCatalog(string catalogId)
        {
            if (_instances.TryGetValue(catalogId, out var existing))
            {
                return (DefinitionCatalog)existing;
            }

            var catalog = ScriptableObject.CreateInstance<DefinitionCatalog>();
            catalog.name = catalogId;
            _instances.Add(catalogId, catalog);
            return catalog;
        }

        public void Dispose()
        {
            foreach (var instance in _instances.Values)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            _instances.Clear();
        }
    }
}
