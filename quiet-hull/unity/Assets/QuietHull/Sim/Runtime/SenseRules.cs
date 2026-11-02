using System;

namespace QuietHull.Sim
{
    /// <summary>The physics of every sense channel. Immutable; the balance runner sweeps these.</summary>
    public sealed class SenseRules
    {
        private readonly ChannelRules[] _channels;

        public SenseRules(ChannelRules sound, ChannelRules vibration, ChannelRules heat)
        {
            _channels = new[]
            {
                sound ?? throw new ArgumentNullException(nameof(sound)),
                vibration ?? throw new ArgumentNullException(nameof(vibration)),
                heat ?? throw new ArgumentNullException(nameof(heat))
            };
        }

        /// <summary>
        /// Sound: 2 per hop, +3 through a closed door, stopped by a sealed one. Vibration: 1 per hop, doors don't
        /// matter. Heat: 3 per hop, +2 through any shut door, never fully blocked.
        /// </summary>
        public static SenseRules Default { get; } = new SenseRules(
            new ChannelRules(2, 3, sealedBlocks: true),
            new ChannelRules(1, 0, sealedBlocks: false),
            new ChannelRules(3, 2, sealedBlocks: false));

        public ChannelRules Of(SenseChannel channel) => _channels[(int)channel];

        /// <summary>The same rules with a different sound channel (keeps <see cref="NoiseRules"/> working).</summary>
        public SenseRules WithSound(NoiseRules noise) =>
            new SenseRules(new ChannelRules(noise.LossPerHop, noise.ClosedDoorLoss, true), Of(SenseChannel.Vibration), Of(SenseChannel.Heat));
    }
}
