using System;

namespace QuietHull.Sim
{
    /// <summary>All the tuning a simulation runs on. Immutable; the balance runner sweeps these.</summary>
    public sealed class SimRules
    {
        public SimRules(NoiseRules noise, AwarenessRules awareness)
        {
            Noise = noise ?? throw new ArgumentNullException(nameof(noise));
            Awareness = awareness ?? throw new ArgumentNullException(nameof(awareness));
        }

        public static SimRules Default { get; } = new SimRules(NoiseRules.Default, AwarenessRules.Default);

        public NoiseRules Noise { get; }
        public AwarenessRules Awareness { get; }
    }
}
