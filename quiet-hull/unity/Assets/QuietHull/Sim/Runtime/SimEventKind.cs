namespace QuietHull.Sim
{
    public enum SimEventKind
    {
        /// <summary>Journalled: the hive reached a new stage.</summary>
        HiveStageChanged = 0,
        /// <summary>Transient: an alien finished crossing into a room (dispatched, not journalled).</summary>
        AlienEnteredRoom = 1
    }
}
