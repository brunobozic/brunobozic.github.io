namespace QuietHull.Sim
{
    /// <summary>What aliens can sense, beyond sight. Each travels through a hulk differently.</summary>
    public enum SenseChannel
    {
        /// <summary>Through doors: closed doors muffle it, sealed doors stop it.</summary>
        Sound = 0,
        /// <summary>Through the hull: ignores doors, fades slowly. Heavy loads, machinery, footsteps.</summary>
        Vibration = 1,
        /// <summary>Stays local, soaks slowly through doors (a sealed door is just a closed one to heat).</summary>
        Heat = 2
    }

    /// <summary>Helpers for iterating the channels.</summary>
    public static class SenseChannels
    {
        public const int Count = 3;
    }
}
