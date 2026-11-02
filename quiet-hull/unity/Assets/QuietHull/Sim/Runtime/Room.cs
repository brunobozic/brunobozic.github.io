namespace QuietHull.Sim
{
    /// <summary>
    /// A room: its key, display name and ambient levels per sense channel (machinery that roars, shakes or
    /// runs hot masks weaker signals of the same kind).
    /// </summary>
    public sealed class Room
    {
        private readonly int[] _ambient;

        internal Room(RoomId id, string key, string name, int ambientNoise, int ambientVibration, int ambientHeat)
        {
            Id = id;
            Key = key;
            Name = name;
            _ambient = new[] { ambientNoise, ambientVibration, ambientHeat };
        }

        public RoomId Id { get; }
        public string Key { get; }
        public string Name { get; }
        public int AmbientNoise => _ambient[(int)SenseChannel.Sound];
        public int AmbientVibration => _ambient[(int)SenseChannel.Vibration];
        public int AmbientHeat => _ambient[(int)SenseChannel.Heat];

        public int Ambient(SenseChannel channel) => _ambient[(int)channel];
    }
}
