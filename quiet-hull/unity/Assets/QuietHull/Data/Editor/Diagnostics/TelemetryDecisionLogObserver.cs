using System;
using System.Diagnostics;
using System.Globalization;
using Serilog;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Writes every recorded story decision to Serilog (structured, so Graylog can filter by decision, option,
    /// verdict and mission) and as an OpenTelemetry span with <c>quiethull.decision.*</c> tags (Jaeger, Sentry).
    /// Used by the editor tools and the balance runner; the game layer registers the same observer at startup.
    /// Stateless apart from its injected logger and source, so it is safe to call from any thread.
    /// </summary>
    public sealed class TelemetryDecisionLogObserver : IDecisionLogObserver
    {
        public const string ActivityName = "story.decision";

        private readonly ILogger _log;
        private readonly ActivitySource _source;

        public TelemetryDecisionLogObserver(ILogger logger, ActivitySource source)
        {
            _log = (logger ?? throw new ArgumentNullException(nameof(logger))).ForContext<TelemetryDecisionLogObserver>();
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>The observer wired to the editor's Serilog logger and ActivitySource.</summary>
        public static TelemetryDecisionLogObserver ForEditor() => new TelemetryDecisionLogObserver(EditorTelemetry.Logger, EditorTelemetry.Source);

        public void OnRecorded(DecisionRecord record)
        {
            using (var activity = _source.StartActivity(ActivityName, ActivityKind.Internal))
            {
                activity?.SetTag("quiethull.decision.id", record.DecisionId);
                activity?.SetTag("quiethull.decision.option", record.OptionId);
                activity?.SetTag("quiethull.decision.sequence", record.Sequence);
                activity?.SetTag("quiethull.decision.mission", record.Mission);
                activity?.SetTag("quiethull.decision.verdict", record.Verdict.ToString());
                activity?.SetTag("quiethull.decision.predicted", record.Verdict == ForecastVerdict.Unforecast
                    ? "none"
                    : record.Predicted.ToString("0.00", CultureInfo.InvariantCulture));

                var log = _log
                    .ForContext("DecisionId", record.DecisionId)
                    .ForContext("OptionId", record.OptionId)
                    .ForContext("DecisionSequence", record.Sequence)
                    .ForContext("Mission", record.Mission)
                    .ForContext("Verdict", record.Verdict)
                    .ForContext("Predicted", record.Predicted)
                    .ForContext("TraceId", activity?.TraceId.ToString() ?? string.Empty)
                    .ForContext("SpanId", activity?.SpanId.ToString() ?? string.Empty);

                if (record.Verdict == ForecastVerdict.Unforecast)
                {
                    // The one outcome the company has no number for: worth a warning in any dashboard.
                    log.Warning("Decision {DecisionId} = {OptionId} (#{DecisionSequence}, mission {Mission}) is unforecast", record.DecisionId, record.OptionId, record.Sequence, record.Mission);
                }
                else
                {
                    log.Information("Decision {DecisionId} = {OptionId} (#{DecisionSequence}, mission {Mission}): predicted {Predicted:0.00}, {Verdict}",
                        record.DecisionId, record.OptionId, record.Sequence, record.Mission, record.Predicted, record.Verdict);
                }
            }
        }
    }
}
