namespace QuietHull.Sim
{
    /// <summary>A room the hive senses from (a nest, a dormant cluster), and what it senses with.</summary>
    public readonly struct HiveListener
    {
        public HiveListener(RoomId room, SenseWeights weights)
        {
            Room = room;
            Weights = weights;
        }

        public RoomId Room { get; }
        public SenseWeights Weights { get; }
    }
}
