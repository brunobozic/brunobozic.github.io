namespace QuietHull.Data
{
    /// <summary>
    /// Told about every decision as it is recorded. The game layer implements it to write the decision to
    /// Serilog and to tag the current OpenTelemetry span (the data layer itself has no logging dependencies).
    /// Called on the recording thread, outside the log's lock.
    /// </summary>
    public interface IDecisionLogObserver
    {
        void OnRecorded(DecisionRecord record);
    }
}
