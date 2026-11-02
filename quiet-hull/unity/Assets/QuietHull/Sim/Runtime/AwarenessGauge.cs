using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// The hive's Awareness: heard noise fills it, silence drains it, and crossing a threshold moves the hive to
    /// the next stage for good. Fixed-point (thousandths) so every platform gets the same run. Single-threaded,
    /// advanced by the simulation tick.
    /// </summary>
    public sealed class AwarenessGauge
    {
        private const int Scale = 1000;

        private readonly AwarenessRules _rules;
        private int _milli;

        public AwarenessGauge(AwarenessRules rules) => _rules = rules ?? throw new ArgumentNullException(nameof(rules));

        /// <summary>Raised once per stage reached, in order (from, to).</summary>
        public event Action<HiveStage, HiveStage> StageChanged;

        public float Value => _milli / (float)Scale;
        public HiveStage Stage { get; private set; } = HiveStage.Dormant;

        /// <summary>Advances one tick with the total noise the hive heard this tick.</summary>
        public void Tick(int heardNoise)
        {
            if (heardNoise > 0)
            {
                _milli += heardNoise * _rules.GainPerHeardPointPerSecond * Scale / SimTime.TicksPerSecond;
            }
            else
            {
                _milli -= _rules.DecayPerSecond * Scale / SimTime.TicksPerSecond;
            }

            _milli = Math.Max(0, Math.Min(_milli, _rules.Maximum * Scale));

            if (Stage == HiveStage.Dormant && _milli >= _rules.RestlessAt * Scale)
            {
                Advance(HiveStage.Restless);
            }

            if (Stage == HiveStage.Restless && _milli >= _rules.HuntAt * Scale)
            {
                Advance(HiveStage.Hunt);
            }
        }

        private void Advance(HiveStage to)
        {
            var from = Stage;
            Stage = to;
            StageChanged?.Invoke(from, to);
        }
    }
}
