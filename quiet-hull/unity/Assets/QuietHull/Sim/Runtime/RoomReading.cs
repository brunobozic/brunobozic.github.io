namespace QuietHull.Sim
{
    /// <summary>
    /// What the player's map shows for one room: nothing unless a sensor covers it; otherwise each channel's
    /// level and whether the arriving signal is masked by the room's own background.
    /// </summary>
    public readonly struct RoomReading
    {
        private readonly int _sound, _vibration, _heat, _ambientSound, _ambientVibration, _ambientHeat;

        internal RoomReading(bool covered, int sound, int vibration, int heat, int ambientSound, int ambientVibration, int ambientHeat)
        {
            Covered = covered;
            _sound = sound;
            _vibration = vibration;
            _heat = heat;
            _ambientSound = ambientSound;
            _ambientVibration = ambientVibration;
            _ambientHeat = ambientHeat;
        }

        public bool Covered { get; }

        /// <summary>The arriving signal (from emitters), before masking.</summary>
        public int Arrival(SenseChannel c) => c == SenseChannel.Sound ? _sound : c == SenseChannel.Vibration ? _vibration : _heat;

        public int Ambient(SenseChannel c) => c == SenseChannel.Sound ? _ambientSound : c == SenseChannel.Vibration ? _ambientVibration : _ambientHeat;

        /// <summary>What the room reads overall: the louder of the arrival and its own background.</summary>
        public int Level(SenseChannel c) => Arrival(c) > Ambient(c) ? Arrival(c) : Ambient(c);

        /// <summary>Something is arriving, but the room's background hides it.</summary>
        public bool Masked(SenseChannel c) => Arrival(c) > 0 && Arrival(c) <= Ambient(c);
    }
}
