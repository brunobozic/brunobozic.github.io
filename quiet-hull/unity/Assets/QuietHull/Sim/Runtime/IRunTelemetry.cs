namespace QuietHull.Sim
{
    /// <summary>
    /// Per-tick measurements for the fun gates (Awareness timeline, what the hive heard). The core stays
    /// dependency-free: a Serilog / OpenTelemetry adapter implements this outside the simulation, and should
    /// aggregate rather than log every tick.
    /// </summary>
    public interface IRunTelemetry
    {
        void OnTick(int tick, int heardByHive, float awareness);
    }
}
