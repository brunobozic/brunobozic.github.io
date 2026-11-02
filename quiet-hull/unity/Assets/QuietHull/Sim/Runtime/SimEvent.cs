namespace QuietHull.Sim
{
    /// <summary>Something that happened in a run, stamped with its tick. The telemetry, audio, UI and replay feed.</summary>
    public readonly struct SimEvent
    {
        public SimEvent(int tick, SimEventKind kind, HiveStage stage)
            : this(tick, kind, stage, default, default)
        {
        }

        public SimEvent(int tick, SimEventKind kind, HiveStage stage, RoomId room, AlienHandle alien)
        {
            Tick = tick;
            Kind = kind;
            Stage = stage;
            Room = room;
            Alien = alien;
        }

        public int Tick { get; }
        public SimEventKind Kind { get; }
        public HiveStage Stage { get; }
        public RoomId Room { get; }
        public AlienHandle Alien { get; }

        public override string ToString() => Kind == SimEventKind.AlienEnteredRoom ? $"{Tick}:{Kind}:{Alien}>{Room}" : $"{Tick}:{Kind}:{Stage}";
    }
}
