namespace QuietHull.Data
{
    /// <summary>The outcome of one Cartomancer operation: the new card (or the old one, unchanged, on failure) and what it cost.</summary>
    public readonly struct CartomancyResult
    {
        private CartomancyResult(bool success, OmenCardInstance card, int scripCost, string reason)
        {
            Success = success;
            Card = card;
            ScripCost = scripCost;
            Reason = reason ?? string.Empty;
        }

        public bool Success { get; }
        public OmenCardInstance Card { get; }
        public int ScripCost { get; }

        /// <summary>Why it failed (empty on success); shown to the player at the table.</summary>
        public string Reason { get; }

        public static CartomancyResult Ok(OmenCardInstance card, int scripCost = 0) => new CartomancyResult(true, card, scripCost, null);

        public static CartomancyResult Fail(OmenCardInstance card, string reason) => new CartomancyResult(false, card, 0, reason);

        public override string ToString() => Success ? $"ok: {Card} (−{ScripCost} Scrip)" : $"refused: {Reason}";
    }
}
