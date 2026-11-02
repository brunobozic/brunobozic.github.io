using System;

namespace QuietHull.Data
{
    /// <summary>The handicaps a family's cards can draw when the Cartomancer rerolls or deepens them.</summary>
    [Serializable]
    public struct OmenHandicapPool
    {
        public OmenFamily family;
        public OmenEffect[] handicaps;

        public OmenHandicapPool(OmenFamily family, params OmenEffect[] handicaps)
        {
            this.family = family;
            this.handicaps = handicaps ?? Array.Empty<OmenEffect>();
        }
    }
}
