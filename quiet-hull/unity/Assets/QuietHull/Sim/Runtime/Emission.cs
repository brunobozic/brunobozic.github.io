namespace QuietHull.Sim
{
    /// <summary>What something gives off: a Joe cutting (loud, hot), a loaded hauler (quiet, heavy), a fire (hot).</summary>
    public readonly struct Emission
    {
        public Emission(int sound, int vibration, int heat)
        {
            Sound = sound < 0 ? 0 : sound;
            Vibration = vibration < 0 ? 0 : vibration;
            Heat = heat < 0 ? 0 : heat;
        }

        public int Sound { get; }
        public int Vibration { get; }
        public int Heat { get; }

        public int Of(SenseChannel channel) => channel == SenseChannel.Sound ? Sound : channel == SenseChannel.Vibration ? Vibration : Heat;
    }
}
