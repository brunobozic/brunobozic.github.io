using System;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// The commander's interface from company-clean to biomechanical. Values mirror the "Theme: the guided
    /// monster" tab: Stage I is the company's industrial UI (charcoal, amber, sharp frames); by Stage V the screens
    /// are blue-black, bone and cold chrome, ribbed and breathing. Every blend keeps WCAG AA contrast.
    /// </summary>
    public static class UiThemeBuilder
    {
        public const string CatalogId = "cat_ui_theme";
        private const string Folder = "UI";

        public static UiThemeContent Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var make = new DefinitionMaker(factory);
            var theme = make.Def<UiThemeDefinition>("ui_theme_commander", Folder, "Commander interface",
                "Drifts with the infection, from company-industrial to biomechanical.", "ui", "theme");

            UiPalette P(string bg, string surface, string frame, string text, string muted, string accent, string danger, string warning,
                float grain, float pulse, float sheen, float organic) => new UiPalette
            {
                background = ColorMath.FromHex(bg),
                surface = ColorMath.FromHex(surface),
                frame = ColorMath.FromHex(frame),
                textPrimary = ColorMath.FromHex(text),
                textMuted = ColorMath.FromHex(muted),
                accent = ColorMath.FromHex(accent),
                danger = ColorMath.FromHex(danger),
                warning = ColorMath.FromHex(warning),
                grain = grain,
                pulse = pulse,
                sheen = sheen,
                organicFrames = organic
            };

            theme.stages = new[]
            {
                // I: company-industrial. Charcoal, warm off-white, amber CRT accent, sharp rectangular frames.
                P("#121416", "#1C2024", "#6E7A80", "#E8E2D0", "#A3ABB0", "#F2A541", "#E5484D", "#F5C542", 0.05f, 0f, 0f, 0f),
                // II: almost the same. A little grain; the amber cools toward bone; frame corners soften.
                P("#121312", "#1B1E1C", "#747A70", "#E6DECB", "#A2A79E", "#D9A85A", "#E04E4A", "#EDC24F", 0.12f, 0f, 0.05f, 0.1f),
                // III: biomechanical begins. Greenish graphite, bone text, tarnished-brass accent, ribbed borders.
                P("#0E100F", "#181B19", "#5E6B5F", "#E3D9C1", "#9EA497", "#C2B280", "#D85B50", "#D9B85A", 0.2f, 0.1f, 0.2f, 0.45f),
                // IV: blue-black and pale chrome; panels breathe; scrollbars like tendons.
                P("#0B0D0E", "#15191B", "#4E5A5E", "#DDD3BC", "#979E99", "#A9B7A8", "#D2605A", "#CDB063", 0.25f, 0.3f, 0.45f, 0.75f),
                // V: fully biomechanical. Black chrome, wet highlights, ivory text, vertebral frames, a slow heartbeat.
                P("#08090A", "#111416", "#3E4A50", "#D8CDB4", "#939A97", "#9FB3B8", "#CE6158", "#C7AA66", 0.3f, 0.5f, 0.7f, 1f)
            };

            return new UiThemeContent { CommanderTheme = theme, Catalog = make.WriteCatalog(CatalogId), All = make.All };
        }
    }
}
