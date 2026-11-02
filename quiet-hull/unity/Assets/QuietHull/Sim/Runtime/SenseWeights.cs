namespace QuietHull.Sim
{
    /// <summary>
    /// How strongly a listener reacts to each channel, in percent. Alien types differ: Crawlers feel the deck,
    /// Stalkers see heat, the hive at large hears everything. Integer percent keeps the maths deterministic.
    /// </summary>
    public readonly struct SenseWeights
    {
        public SenseWeights(int soundPercent, int vibrationPercent, int heatPercent)
        {
            SoundPercent = soundPercent;
            VibrationPercent = vibrationPercent;
            HeatPercent = heatPercent;
        }

        public static SenseWeights All => new SenseWeights(100, 100, 100);
        public static SenseWeights EarsOnly => new SenseWeights(100, 0, 0);
        public static SenseWeights VibrationOnly => new SenseWeights(0, 100, 0);
        public static SenseWeights HeatOnly => new SenseWeights(0, 0, 100);

        public int SoundPercent { get; }
        public int VibrationPercent { get; }
        public int HeatPercent { get; }

        public int Of(SenseChannel channel) =>
            channel == SenseChannel.Sound ? SoundPercent : channel == SenseChannel.Vibration ? VibrationPercent : HeatPercent;
    }
}
