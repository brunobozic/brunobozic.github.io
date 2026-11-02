using System;
using System.Diagnostics;
using Serilog.Context;
using UnityEditor;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Writes a content pack to assets: builds it through an <see cref="AssetDefinitionFactory"/>,
    /// saves, validates the catalog and reports. Every step is logged and traced.
    /// </summary>
    public static class ContentGenerator
    {
        public const string DefaultRoot = "Assets/QuietHull/Content";

        /// <summary>The outcome of one generation run.</summary>
        public readonly struct Result
        {
            public Result(DefinitionCatalog catalog, ValidationReport report, int created, int updated)
            {
                Catalog = catalog;
                Report = report;
                Created = created;
                Updated = updated;
            }

            public DefinitionCatalog Catalog { get; }
            public ValidationReport Report { get; }
            public int Created { get; }
            public int Updated { get; }
        }

        public static Result Run(string packName, string rootFolder, Func<IDefinitionFactory, DefinitionCatalog> build)
        {
            if (string.IsNullOrWhiteSpace(packName))
            {
                throw new ArgumentException("Pack name is required.", nameof(packName));
            }

            if (build == null)
            {
                throw new ArgumentNullException(nameof(build));
            }

            var log = EditorTelemetry.For<ContentGeneratorMarker>();
            var stopwatch = Stopwatch.StartNew();

            using (var activity = EditorTelemetry.Source.StartActivity("content.generate"))
            using (LogContext.PushProperty("Tool", "ContentGenerator"))
            using (LogContext.PushProperty("ContentPack", packName))
            using (LogContext.PushProperty("RootFolder", rootFolder))
            {
                activity?.SetTag("quiethull.tool", "content_generator");
                activity?.SetTag("quiethull.content.pack", packName);
                activity?.SetTag("quiethull.asset.root", rootFolder);

                log.Information("Generating content pack {ContentPack} into {RootFolder}", packName, rootFolder);

                var factory = new AssetDefinitionFactory(rootFolder, log);
                DefinitionCatalog catalog;

                // No StartAssetEditing batching: folders created inside a batch aren't visible to
                // IsValidFolder until it ends, which would create duplicate folders.
                try
                {
                    using (var buildActivity = EditorTelemetry.Source.StartActivity("content.build"))
                    {
                        buildActivity?.SetTag("quiethull.content.pack", packName);
                        catalog = build(factory);
                    }
                }
                catch (Exception exception)
                {
                    activity.RecordException(exception);
                    log.Error(exception, "Content pack {ContentPack} failed after {ElapsedMs} ms", packName, stopwatch.ElapsedMilliseconds);
                    throw;
                }

                foreach (var asset in factory.Touched)
                {
                    EditorUtility.SetDirty(asset);
                }

                AssetDatabase.SaveAssets();

                ValidationReport report;
                using (var validation = EditorTelemetry.Source.StartActivity("content.validate"))
                {
                    report = catalog.Validate();
                    validation?.SetTag("quiethull.catalog.id", catalog.CatalogId);
                    validation?.SetTag("quiethull.validation.errors", report.ErrorCount);
                    validation?.SetTag("quiethull.validation.warnings", report.WarningCount);
                    ValidationLogger.Log(log, catalog.CatalogId, report);
                }

                activity?.SetTag("quiethull.catalog.id", catalog.CatalogId);
                activity?.SetTag("quiethull.definitions.count", catalog.Definitions.Count);
                activity?.SetTag("quiethull.assets.created", factory.CreatedCount);
                activity?.SetTag("quiethull.assets.updated", factory.UpdatedCount);
                activity?.SetStatus(report.HasErrors ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

                log.Information(
                    "Content pack {ContentPack} generated: {DefinitionCount} definitions, {CreatedCount} created, {UpdatedCount} updated, {ErrorCount} errors, {WarningCount} warnings in {ElapsedMs} ms",
                    packName, catalog.Definitions.Count, factory.CreatedCount, factory.UpdatedCount, report.ErrorCount, report.WarningCount, stopwatch.ElapsedMilliseconds);

                return new Result(catalog, report, factory.CreatedCount, factory.UpdatedCount);
            }
        }

        internal static void ShowResult(string packName, Result result)
        {
            var title = result.Report.HasErrors ? $"{packName}: data has errors" : $"{packName}: data generated";
            EditorUtility.DisplayDialog(title,
                $"Created {result.Created}, updated {result.Updated}.\nErrors: {result.Report.ErrorCount}, warnings: {result.Report.WarningCount}.\nSee the Console for details.",
                "OK");
        }

        // Static classes can't be type arguments; this gives the logger a SourceContext.
        private sealed class ContentGeneratorMarker
        {
        }
    }

    /// <summary>Menu items for every content pack.</summary>
    public static class ContentGeneratorMenu
    {
        [MenuItem("Quiet Hull/Content/Generate Prologue", priority = 10)]
        private static void GeneratePrologue() =>
            ContentGenerator.ShowResult("Prologue", ContentGenerator.Run("Prologue", ContentGenerator.DefaultRoot,
                factory => PrologueContentBuilder.Build(factory).Catalog));

        [MenuItem("Quiet Hull/Content/Generate Sample: Tremolino sun and belt", priority = 30)]
        private static void GenerateSunwheel() =>
            ContentGenerator.ShowResult("Tremolino sample", ContentGenerator.Run("Tremolino sample", ContentGenerator.DefaultRoot,
                factory =>
                {
                    SunwheelSampleBuilder.Build(factory);
                    return ((IDefinitionFactory)factory).GetOrCreateCatalog(SunwheelSampleBuilder.CatalogId);
                }));

        [MenuItem("Quiet Hull/Content/Generate Mission 2 (Cold Sleep)", priority = 11)]
        private static void GenerateColdSleep() =>
            // Same root as the prologue, so shared definitions (materials, sentries, aliens) stay single assets.
            ContentGenerator.ShowResult("Cold Sleep", ContentGenerator.Run("Cold Sleep", ContentGenerator.DefaultRoot,
                factory => ColdSleepContentBuilder.Build(factory).Catalog));
    }
}
