using System.Diagnostics;
using Serilog.Context;
using UnityEditor;

namespace QuietHull.Data.Editor
{
    /// <summary>Menu: Quiet Hull ▸ Validate All Catalogs. Validates every DefinitionCatalog in the project.</summary>
    public static class CatalogValidationMenu
    {
        [MenuItem("Quiet Hull/Validate All Catalogs", priority = 20)]
        public static void ValidateAll()
        {
            var log = EditorTelemetry.For<CatalogValidationMarker>();
            var totalErrors = 0;
            var totalWarnings = 0;

            using (var activity = EditorTelemetry.Source.StartActivity("catalogs.validate_all"))
            using (LogContext.PushProperty("Tool", "CatalogValidation"))
            {
                var guids = AssetDatabase.FindAssets("t:" + nameof(DefinitionCatalog));
                activity?.SetTag("quiethull.catalogs.count", guids.Length);

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var catalog = AssetDatabase.LoadAssetAtPath<DefinitionCatalog>(path);
                    if (catalog == null)
                    {
                        continue;
                    }

                    using (var catalogActivity = EditorTelemetry.Source.StartActivity("catalog.validate"))
                    using (LogContext.PushProperty("AssetPath", path))
                    {
                        catalogActivity?.SetTag("quiethull.catalog.id", catalog.CatalogId);
                        catalogActivity?.SetTag("quiethull.asset.path", path);
                        catalogActivity?.SetTag("quiethull.definitions.count", catalog.Definitions.Count);

                        var report = catalog.Validate();
                        catalogActivity?.SetTag("quiethull.validation.errors", report.ErrorCount);
                        catalogActivity?.SetTag("quiethull.validation.warnings", report.WarningCount);
                        catalogActivity?.SetStatus(report.HasErrors ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

                        ValidationLogger.Log(log, catalog.CatalogId, report);
                        totalErrors += report.ErrorCount;
                        totalWarnings += report.WarningCount;
                    }
                }

                activity?.SetTag("quiethull.validation.errors", totalErrors);
                activity?.SetTag("quiethull.validation.warnings", totalWarnings);
                activity?.SetStatus(totalErrors > 0 ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
                log.Information("Validated {CatalogCount} catalogs: {ErrorCount} errors, {WarningCount} warnings", guids.Length, totalErrors, totalWarnings);
            }
        }

        private sealed class CatalogValidationMarker
        {
        }
    }
}
