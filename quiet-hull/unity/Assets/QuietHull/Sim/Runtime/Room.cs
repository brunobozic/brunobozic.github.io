namespace QuietHull.Sim
{
    /// <summary>A room: its key, display name and ambient noise (machinery that masks quieter sounds).</summary>
    public sealed class Room
    {
        internal Room(RoomId id, string key, string name, int ambientNoise)
        {
            Id = id;
            Key = key;
            Name = name;
            AmbientNoise = ambientNoise;
        }

        public RoomId Id { get; }
        public string Key { get; }
        public string Name { get; }
        public int AmbientNoise { get; }
    }
}
