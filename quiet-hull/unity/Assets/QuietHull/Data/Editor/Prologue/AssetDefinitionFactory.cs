using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using UnityEditor;
using UnityEngine;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Asset-backed factory: loads the existing asset at <c>{root}/{folder}/{id}.asset</c> or creates it.
    /// Reusing the existing object keeps GUIDs stable, so scenes and prefabs that reference the
    /// definitions survive a regeneration. Editor main thread only (AssetDatabase is not thread-safe).
    /// </summary>
    public sealed class AssetDefinitionFactory : IDefinitionFactory
    {
        private readonly string _root;
        private readonly ILogger _log;
        private readonly List<ScriptableObject> _touched = new List<ScriptableObject>();

        public AssetDefinitionFactory(string rootFolder, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || !rootFolder.StartsWith("Assets", StringComparison.Ordinal))
            {
                throw new ArgumentException("Root folder must be a project path under Assets/.", nameof(rootFolder));
            }

            _root = rootFolder.TrimEnd('/');
            _log = (logger ?? throw new ArgumentNullException(nameof(logger))).ForContext<AssetDefinitionFactory>();
        }

        public int CreatedCount { get; private set; }
        public int UpdatedCount { get; private set; }

        /// <summary>Every asset this factory handed out; the generator marks them dirty after configuring them.</summary>
        public IReadOnlyList<ScriptableObject> Touched => _touched;

        public T GetOrCreate<T>(string id, string folder) where T : DefinitionBase
        {
            var directory = EnsureFolder(string.IsNullOrEmpty(folder) ? _root : $"{_root}/{folder}");
            return GetOrCreateAt<T>($"{directory}/{id}.asset", id);
        }

        public DefinitionCatalog GetOrCreateCatalog(string catalogId) => GetOrCreateAt<DefinitionCatalog>($"{EnsureFolder(_root)}/{catalogId}.asset", catalogId);

        private T GetOrCreateAt<T>(string path, string id) where T : ScriptableObject
        {
            using (var activity = EditorTelemetry.Source.StartActivity("definition.get_or_create"))
            {
                activity?.SetTag("quiethull.definition.id", id);
                activity?.SetTag("quiethull.definition.type", typeof(T).Name);
                activity?.SetTag("quiethull.asset.path", path);

                var existing = AssetDatabase.LoadMainAssetAtPath(path);
                if (existing != null && !(existing is T))
                {
                    var error = new InvalidOperationException(
                        $"Asset at '{path}' is a {existing.GetType().Name}, expected {typeof(T).Name}. Move or delete it, then regenerate.");
                    activity.RecordException(error);
                    throw error;
                }

                if (existing is T found)
                {
                    UpdatedCount++;
                    activity?.SetTag("quiethull.asset.action", "updated");
                    _touched.Add(found);
                    _log.Verbose("Reusing {DefinitionType} {DefinitionId} at {AssetPath}", typeof(T).Name, id, path);
                    return found;
                }

                var created = ScriptableObject.CreateInstance<T>();
                created.name = id;
                AssetDatabase.CreateAsset(created, path);
                CreatedCount++;
                activity?.SetTag("quiethull.asset.action", "created");
                _touched.Add(created);
                _log.Information("Created {DefinitionType} {DefinitionId} at {AssetPath}", typeof(T).Name, id, path);
                return created;
            }
        }

        private static string EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }

            var slash = path.LastIndexOf('/');
            var parent = EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
            return path;
        }
    }
}
