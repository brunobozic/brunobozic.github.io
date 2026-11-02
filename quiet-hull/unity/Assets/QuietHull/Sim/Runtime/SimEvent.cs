namespace QuietHull.Sim
{
    /// <summary>Something that happened in a run, stamped with its tick. The telemetry and replay feed.</summary>
    public readonly struct SimEvent
    {
        public SimEvent(int tick, SimEventKind kind, HiveStage stage)
        {
            Tick = tick;
            Kind = kind;
            Stage = stage;
        }

        public int Tick { get; }
        public SimEventKind Kind { get; }
        public HiveStage Stage { get; }

        public override string ToString() => $"{Tick}:{Kind}:{Stage}";
    }
}
