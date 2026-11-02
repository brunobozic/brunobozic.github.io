namespace QuietHull.Sim
{
    public enum DoorState
    {
        Open = 0,
        /// <summary>Closed: muffles sound; aliens can still force it.</summary>
        Closed = 1,
        /// <summary>Welded or sealed: blocks sound; must be cut or broken through.</summary>
        Sealed = 2
    }
}
