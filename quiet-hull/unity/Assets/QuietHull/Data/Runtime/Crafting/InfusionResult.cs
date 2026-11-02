namespace QuietHull.Data
{
    /// <summary>The outcome of an amalgam infusion. The item is Infused either way: no Catalyst can touch it again.</summary>
    public readonly struct InfusionResult
    {
        public InfusionResult(InfusionOutcome outcome, Mark? newMark, bool socketsProtected)
        {
            Outcome = outcome;
            NewMark = newMark;
            SocketsProtected = socketsProtected;
        }

        public InfusionOutcome Outcome { get; }

        /// <summary>Overgrowth only: the Mark past the item's normal maximum.</summary>
        public Mark? NewMark { get; }

        /// <summary>Rare and better: Rot can't touch sockets.</summary>
        public bool SocketsProtected { get; }

        public bool IsInfused => true;

        public override string ToString() => NewMark.HasValue ? $"{Outcome} → {NewMark}" : Outcome.ToString();
    }
}
