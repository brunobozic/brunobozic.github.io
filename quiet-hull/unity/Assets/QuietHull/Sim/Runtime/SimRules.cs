using System;

namespace QuietHull.Sim
{
    /// <summary>All the tuning a simulation runs on. Immutable; the balance runner sweeps these.</summary>
    public sealed class SimRules
    {
        public SimRules(SenseRules senses, AwarenessRules awareness)
            : this(senses, awareness, SwarmRules.Default)
        {
        }

        public SimRules(SenseRules senses, AwarenessRules awareness, SwarmRules swarm)
        {
            Senses = senses ?? throw new ArgumentNullException(nameof(senses));
            Awareness = awareness ?? throw new ArgumentNullException(nameof(awareness));
            Swarm = swarm ?? throw new ArgumentNullException(nameof(swarm));
        }

        /// <summary>Sound tuned by <paramref name="noise"/>; vibration and heat at their defaults.</summary>
        public SimRules(NoiseRules noise, AwarenessRules awareness)
            : this(SenseRules.Default.WithSound(noise ?? throw new ArgumentNullException(nameof(noise))), awareness)
        {
        }

        public static SimRules Default { get; } = new SimRules(SenseRules.Default, AwarenessRules.Default);

        public SenseRules Senses { get; }
        public AwarenessRules Awareness { get; }
        public SwarmRules Swarm { get; }
    }
}
