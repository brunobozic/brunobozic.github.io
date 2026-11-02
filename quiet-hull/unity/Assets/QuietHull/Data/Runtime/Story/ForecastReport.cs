using System;
using System.Collections.Generic;
using System.Globalization;

namespace QuietHull.Data
{
    /// <summary>
    /// The company's file on the candidate: the player's recorded decisions up to a mission, each with the
    /// probability Vashti-Kell assigned to it. Pure; builds a new report every call, so it is thread-safe.
    /// </summary>
    public sealed class ForecastReport
    {
        public const string CandidateHeader = "Candidate M-7, \"Marlow\" (Patna survivor)";
        public const string DeviationNote = "Acceptable deviation.";
        public const string UnforecastMark = "—";

        private ForecastReport(IReadOnlyList<ForecastLine> lines, int expected, int deviations, int unforecast)
        {
            Lines = lines;
            ExpectedCount = expected;
            DeviationCount = deviations;
            UnforecastCount = unforecast;
        }

        public IReadOnlyList<ForecastLine> Lines { get; }
        public int ExpectedCount { get; }
        public int DeviationCount { get; }
        public int UnforecastCount { get; }

        /// <summary>The file as of <paramref name="upToMission"/> (mission 10 shows everything decided by then).</summary>
        public static ForecastReport Build(DecisionLog log, IEnumerable<DecisionPointDefinition> decisions, int upToMission)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var byId = new Dictionary<string, DecisionPointDefinition>(StringComparer.Ordinal);
            foreach (var decision in decisions ?? Array.Empty<DecisionPointDefinition>())
            {
                if (decision != null)
                {
                    byId[decision.Id] = decision;
                }
            }

            var lines = new List<ForecastLine>();
            int expected = 0, deviations = 0, unforecast = 0;
            foreach (var record in log.Records)
            {
                if (record.Mission > upToMission || !byId.TryGetValue(record.DecisionId, out var decision) ||
                    !decision.TryGetOption(record.OptionId, out var option))
                {
                    continue;
                }

                string text;
                switch (record.Verdict)
                {
                    case ForecastVerdict.Unforecast:
                        unforecast++;
                        text = $"{decision.FileLabel}: {option.forecastLine}. Predicted {UnforecastMark}.";
                        break;
                    case ForecastVerdict.Deviation:
                        deviations++;
                        text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}. Predicted {2:0.00}. {3}",
                            decision.FileLabel, option.forecastLine, record.Predicted, DeviationNote);
                        break;
                    default:
                        expected++;
                        text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}. Predicted {2:0.00}.",
                            decision.FileLabel, option.forecastLine, record.Predicted);
                        break;
                }

                lines.Add(new ForecastLine(record, text));
            }

            return new ForecastReport(lines, expected, deviations, unforecast);
        }
    }
}
