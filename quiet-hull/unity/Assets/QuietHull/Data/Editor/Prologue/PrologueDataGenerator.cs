using System;
using System.Diagnostics;
using Serilog;
using Serilog.Context;
using UnityEditor;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Menu: Quiet Hull ▸ Prologue ▸ Generate Data Assets.
    /// Builds every prologue definition from <see cref="PrologueContentBuilder"/>, writes or updates the
    /// assets, saves the catalog and validates it. Safe to run repeatedly.
    /// </summary>
    public static class PrologueDataGenerator
    {
        public const string DefaultRoot = "Assets/QuietHull/Content/Prologue";

        [MenuItem("Quiet Hull/Prologue/Generate Data Assets", priority = 10)]
        private static void GenerateFromMenu()
        {
            var result = Generate(DefaultRoot);
            var title = result.Report.HasErrors ? "Prologue data has errors" : "Prologue data generated";
            EditorUtility.DisplayDialog(title,
                $"Created {result.Created}, updated {result.Updated}.\nErrors: {result.Report.ErrorCount}, warnings: {result.Report.WarningCount}.\nSee the Console for details.",
                "OK");
        }

        public static GenerationResult Generate(string rootFolder)
        {
            var log = EditorTelemetry.For<PrologueDataGeneratorMarker>();
            var stopwatch = Stopwatch.StartNew();

            using (var activity = EditorTelemetry.Source.StartActivity("prologue.generate"))
            using (LogContext.PushProperty("Tool", "PrologueDataGenerator"))
            using (LogContext.PushProperty("RootFolder", rootFolder))
            using (LogContext.PushProperty("CatalogId", PrologueContentBuilder.CatalogId))
            {
                activity?.SetTag("quiethull.tool", "prologue_generator");
                activity?.SetTag("quiethull.asset.root", rootFolder);
                activity?.SetTag("quiethull.catalog.id", PrologueContentBuilder.CatalogId);

                log.Information("Generating prologue data into {RootFolder}", rootFolder);

                var factory = new AssetDefinitionFactory(rootFolder, log);
                PrologueContent content;

                // No StartAssetEditing batching: folders created inside a batch aren't visible to
                // IsValidFolder until it ends, which would create duplicate folders.
                try
                {
                    using (EditorTelemetry.Source.StartActivity("prologue.build"))
                    {
                        content = PrologueContentBuilder.Build(factory);
                    }
                }
                catch (Exception exception)
                {
                    activity.RecordException(exception);
                    log.Error(exception, "Prologue generation failed after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
                    throw;
                }

                foreach (var asset in factory.Touched)
                {
                    EditorUtility.SetDirty(asset);
                }

                AssetDatabase.SaveAssets();

                ValidationReport report;
                using (var validation = EditorTelemetry.Source.StartActivity("prologue.validate"))
                {
                    report = content.Catalog.Validate();
                    validation?.SetTag("quiethull.validation.errors", report.ErrorCount);
                    validation?.SetTag("quiethull.validation.warnings", report.WarningCount);
                    ValidationLogger.Log(log, PrologueContentBuilder.CatalogId, report);
                }

                activity?.SetTag("quiethull.definitions.count", content.All.Count);
                activity?.SetTag("quiethull.assets.created", factory.CreatedCount);
                activity?.SetTag("quiethull.assets.updated", factory.UpdatedCount);
                activity?.SetStatus(report.HasErrors ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

                log.Information(
                    "Prologue data generated: {DefinitionCount} definitions, {CreatedCount} created, {UpdatedCount} updated, {ErrorCount} errors, {WarningCount} warnings in {ElapsedMs} ms",
                    content.All.Count, factory.CreatedCount, factory.UpdatedCount, report.ErrorCount, report.WarningCount, stopwatch.ElapsedMilliseconds);

                return new GenerationResult(content, report, factory.CreatedCount, factory.UpdatedCount);
            }
        }

        public readonly struct GenerationResult
        {
            public GenerationResult(PrologueContent content, ValidationReport report, int created, int updated)
            {
                Content = content;
                Report = report;
                Created = created;
                Updated = updated;
            }

            public PrologueContent Content { get; }
            public ValidationReport Report { get; }
            public int Created { get; }
            public int Updated { get; }
        }

        // Static classes can't be type arguments; this gives the logger a SourceContext.
        private sealed class PrologueDataGeneratorMarker
        {
        }
    }
}
