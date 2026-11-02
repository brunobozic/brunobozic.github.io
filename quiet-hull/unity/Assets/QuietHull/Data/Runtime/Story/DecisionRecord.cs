using System;

namespace QuietHull.Data
{
    /// <summary>One decision as the game logged it. Immutable; safe to share between threads.</summary>
    public readonly struct DecisionRecord : IEquatable<DecisionRecord>
    {
        public DecisionRecord(int sequence, string decisionId, string optionId, int mission, float predicted, ForecastVerdict verdict)
        {
            Sequence = sequence;
            DecisionId = decisionId ?? throw new ArgumentNullException(nameof(decisionId));
            OptionId = optionId ?? throw new ArgumentNullException(nameof(optionId));
            Mission = mission;
            Predicted = predicted;
            Verdict = verdict;
        }

        /// <summary>Order in the campaign, from 1.</summary>
        public int Sequence { get; }
        public string DecisionId { get; }
        public string OptionId { get; }
        public int Mission { get; }

        /// <summary>The company's predicted probability for the chosen option (0 when unforecast).</summary>
        public float Predicted { get; }
        public ForecastVerdict Verdict { get; }

        public bool Equals(DecisionRecord other) =>
            Sequence == other.Sequence && DecisionId == other.DecisionId && OptionId == other.OptionId &&
            Mission == other.Mission && Predicted.Equals(other.Predicted) && Verdict == other.Verdict;

        public override bool Equals(object obj) => obj is DecisionRecord other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Sequence;
                hash = (hash * 397) ^ (DecisionId?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ (OptionId?.GetHashCode() ?? 0);
                return (hash * 397) ^ Mission;
            }
        }

        public override string ToString() => $"#{Sequence} M{Mission} {DecisionId}={OptionId} (p={Predicted:0.00}, {Verdict})";
    }
}
