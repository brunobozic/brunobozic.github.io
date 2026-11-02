namespace QuietHull.Sim
{
    /// <summary>The next room on the way to the goal, and how to get there.</summary>
    public readonly struct FlowStep
    {
        internal FlowStep(RoomId room, bool viaDuct, DoorState door)
        {
            Room = room;
            ViaDuct = viaDuct;
            Door = door;
            HasStep = true;
        }

        /// <summary>False at the goal or where the goal can't be reached.</summary>
        public bool HasStep { get; }
        public RoomId Room { get; }
        public bool ViaDuct { get; }
        public DoorState Door { get; }
    }
}
