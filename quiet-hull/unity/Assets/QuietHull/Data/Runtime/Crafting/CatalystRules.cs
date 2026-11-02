namespace QuietHull.Data
{
    /// <summary>Where each Catalyst works. One table, so the Reliquary, the bench and the Cartomancer agree.</summary>
    public static class CatalystRules
    {
        private const CatalystUse ReliquaryAndBench = CatalystUse.Reliquary | CatalystUse.Bench;

        public static CatalystUse UsesOf(CatalystKind kind)
        {
            switch (kind)
            {
                // Shape what drops; meaningless on an item that already exists.
                case CatalystKind.Focus:
                case CatalystKind.Attune:
                case CatalystKind.Resonance:
                case CatalystKind.Ascension:
                case CatalystKind.Crown:
                case CatalystKind.Lodestone:
                case CatalystKind.Prism:
                case CatalystKind.Augury:
                case CatalystKind.Magnet:
                case CatalystKind.Twin:
                    return CatalystUse.Reliquary;

                case CatalystKind.Imprint:
                case CatalystKind.Socket:
                    return ReliquaryAndBench;

                // These also mutate Omen cards: Reroll, Deepen (Mastercraft), Refine, Corrupt (Hive-tainted).
                case CatalystKind.Mastercraft:
                case CatalystKind.Refine:
                case CatalystKind.Reroll:
                case CatalystKind.HiveTainted:
                    return ReliquaryAndBench | CatalystUse.Card;

                case CatalystKind.Strip:
                case CatalystKind.Temper:
                case CatalystKind.Seal:
                    return CatalystUse.Bench;

                default:
                    return CatalystUse.None;
            }
        }

        public static bool IsMeta(CatalystKind kind) =>
            kind == CatalystKind.Lodestone || kind == CatalystKind.Prism || kind == CatalystKind.Augury ||
            kind == CatalystKind.Magnet || kind == CatalystKind.Twin;
    }
}
