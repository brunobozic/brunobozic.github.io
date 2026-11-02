namespace QuietHull.Data
{
    /// <summary>Accessibility option: how far the interface may drift with the infection.</summary>
    public enum UiDriftSetting
    {
        Full = 0,
        /// <summary>Half the drift; effects (pulse, grain) at half strength.</summary>
        Reduced = 1,
        /// <summary>The Stage I look always.</summary>
        Off = 2
    }
}
