using System;
using Serilog;
using Serilog.Context;

namespace QuietHull.Data.Editor
{
    /// <summary>Writes a <see cref="ValidationReport"/> as structured log events, one per issue.</summary>
    public static class ValidationLogger
    {
        public static void Log(ILogger logger, string catalogId, ValidationReport report)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            foreach (var issue in report.Issues)
            {
                using (LogContext.PushProperty("DefinitionId", issue.DefinitionId))
                using (LogContext.PushProperty("DefinitionType", issue.DefinitionType))
                using (LogContext.PushProperty("Field", issue.Field))
                {
                    if (issue.Severity == ValidationSeverity.Error)
                    {
                        logger.Error("Validation error in {CatalogId}: {DefinitionType} {DefinitionId}.{Field} {ValidationMessage}",
                            catalogId, issue.DefinitionType, issue.DefinitionId, issue.Field, issue.Message);
                    }
                    else
                    {
                        logger.Warning("Validation warning in {CatalogId}: {DefinitionType} {DefinitionId}.{Field} {ValidationMessage}",
                            catalogId, issue.DefinitionType, issue.DefinitionId, issue.Field, issue.Message);
                    }
                }
            }

            logger.Information("Catalog {CatalogId} validated: {ErrorCount} errors, {WarningCount} warnings",
                catalogId, report.ErrorCount, report.WarningCount);
        }
    }
}
