using System;
using System.Collections.Generic;
using System.Threading;

namespace QuietHull.Data
{
    /// <summary>
    /// Every story decision the player makes, in order, with the company's forecast for it. Append-only and
    /// thread-safe: writes take a lock and publish a new immutable snapshot; reads are lock-free. Each decision
    /// point can be recorded once per campaign. Saved and loaded through <see cref="DecisionLogState"/>.
    /// </summary>
    public sealed class DecisionLog
    {
        private readonly object _gate = new object();
        private DecisionRecord[] _records = Array.Empty<DecisionRecord>();
        private IDecisionLogObserver[] _observers = Array.Empty<IDecisionLogObserver>();

        /// <summary>The records so far, in order. A snapshot: later records don't change it.</summary>
        public IReadOnlyList<DecisionRecord> Records => Volatile.Read(ref _records);

        public int Count => Volatile.Read(ref _records).Length;

        public void AddObserver(IDecisionLogObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_gate)
            {
                var next = new IDecisionLogObserver[_observers.Length + 1];
                Array.Copy(_observers, next, _observers.Length);
                next[_observers.Length] = observer;
                Volatile.Write(ref _observers, next);
            }
        }

        public bool TryGet(string decisionId, out DecisionRecord record)
        {
            var snapshot = Volatile.Read(ref _records);
            for (var i = 0; i < snapshot.Length; i++)
            {
                if (string.Equals(snapshot[i].DecisionId, decisionId, StringComparison.Ordinal))
                {
                    record = snapshot[i];
                    return true;
                }
            }

            record = default;
            return false;
        }

        public bool HasDecided(string decisionId) => TryGet(decisionId, out _);

        /// <summary>Records the player's choice at <paramref name="decision"/>. Throws if it was already recorded.</summary>
        public DecisionRecord Record(DecisionPointDefinition decision, string optionId)
        {
            if (decision == null)
            {
                throw new ArgumentNullException(nameof(decision));
            }

            var option = decision.Option(optionId);
            var verdict = decision.Classify(optionId);
            DecisionRecord record;
            lock (_gate)
            {
                for (var i = 0; i < _records.Length; i++)
                {
                    if (string.Equals(_records[i].DecisionId, decision.Id, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{decision.Id} was already decided ({_records[i].OptionId}).");
                    }
                }

                record = new DecisionRecord(_records.Length + 1, decision.Id, option.optionId, decision.Mission,
                    option.unforecast ? 0f : option.predicted, verdict);
                var next = new DecisionRecord[_records.Length + 1];
                Array.Copy(_records, next, _records.Length);
                next[_records.Length] = record;
                Volatile.Write(ref _records, next);
            }

            var observers = Volatile.Read(ref _observers);
            for (var i = 0; i < observers.Length; i++)
            {
                observers[i].OnRecorded(record);
            }

            return record;
        }

        public DecisionLogState ToState()
        {
            var snapshot = Volatile.Read(ref _records);
            var states = new DecisionRecordState[snapshot.Length];
            for (var i = 0; i < snapshot.Length; i++)
            {
                var r = snapshot[i];
                states[i] = new DecisionRecordState
                {
                    sequence = r.Sequence, decisionId = r.DecisionId, optionId = r.OptionId,
                    mission = r.Mission, predicted = r.Predicted, verdict = r.Verdict
                };
            }

            return new DecisionLogState { records = states };
        }

        public static DecisionLog FromState(DecisionLogState state)
        {
            var log = new DecisionLog();
            var saved = state?.records ?? Array.Empty<DecisionRecordState>();
            var records = new List<DecisionRecord>(saved.Length);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in saved)
            {
                if (s == null || string.IsNullOrEmpty(s.decisionId) || string.IsNullOrEmpty(s.optionId) || !seen.Add(s.decisionId))
                {
                    throw new FormatException("the saved decision log has an empty or repeated decision.");
                }

                records.Add(new DecisionRecord(s.sequence, s.decisionId, s.optionId, s.mission, s.predicted, s.verdict));
            }

            records.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));
            log._records = records.ToArray();
            return log;
        }
    }
}
