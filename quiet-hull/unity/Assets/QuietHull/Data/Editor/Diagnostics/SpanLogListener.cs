using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using Serilog.Context;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Fallback span exporter: logs every finished QuietHull editor span with its duration,
    /// status and tags. When the OTEL SDK with an OTLP exporter to Jaeger is added, it subscribes
    /// to the same ActivitySource and this listener can stay as a local trace.
    /// </summary>
    internal static class SpanLogListener
    {
        private static ActivityListener _listener;

        public static void Register(Lazy<ILogger> logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name.StartsWith("QuietHull.", StringComparison.Ordinal),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity => LogSpan(logger.Value, activity)
            };

            ActivitySource.AddActivityListener(listener);
            _listener = listener; // Keep it alive for the editor session.
        }

        private static void LogSpan(ILogger logger, Activity activity)
        {
            var tags = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var tag in activity.TagObjects)
            {
                tags[tag.Key] = tag.Value;
            }

            using (LogContext.PushProperty("TraceId", activity.TraceId.ToHexString()))
            using (LogContext.PushProperty("SpanId", activity.SpanId.ToHexString()))
            using (LogContext.PushProperty("ParentSpanId", activity.ParentSpanId.ToHexString()))
            {
                logger.Debug(
                    "Span {SpanName} finished in {DurationMs:0.0} ms with status {SpanStatus} {@SpanTags}",
                    activity.DisplayName,
                    activity.Duration.TotalMilliseconds,
                    activity.Status,
                    tags);
            }
        }
    }
}
