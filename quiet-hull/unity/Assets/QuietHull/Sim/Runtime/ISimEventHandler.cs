namespace QuietHull.Sim
{
    /// <summary>
    /// Receives the simulation's events (UI, audio, telemetry, replays). Called at the end of the tick that
    /// produced them, in order, on the simulation's thread. Events are structs passed by reference: no allocation.
    /// </summary>
    public interface ISimEventHandler
    {
        void Handle(in SimEvent e);
    }
}
