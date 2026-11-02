using System;

namespace QuietHull.Sim
{
    /// <summary>
    /// How fast the hive's Awareness fills and drains, and where the stages are. Values are whole points;
    /// the gauge works internally in thousandths so it is exact and deterministic.
    /// </summary>
    public sealed class AwarenessRules
    {
        public AwarenessRules(int gainPerHeardPointPerSecond, int decayPerSecond, int restlessAt, int huntAt, int maximum)
        {
            if (gainPerHeardPointPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(gainPerHeardPointPerSecond));
            if (decayPerSecond < 0) throw new ArgumentOutOfRangeException(nameof(decayPerSecond));
            if (restlessAt <= 0 || huntAt <= restlessAt || maximum < huntAt)
            {
                throw new ArgumentException("thresholds must rise: 0 < Restless < Hunt <= maximum.");
            }

            GainPerHeardPointPerSecond = gainPerHeardPointPerSecond;
            DecayPerSecond = decayPerSecond;
            RestlessAt = restlessAt;
            HuntAt = huntAt;
            Maximum = maximum;
        }

        public static AwarenessRules Default { get; } = new AwarenessRules(1, 2, 40, 80, 100);

        public int GainPerHeardPointPerSecond { get; }
        public int DecayPerSecond { get; }
        public int RestlessAt { get; }
        public int HuntAt { get; }
        public int Maximum { get; }
    }
}
