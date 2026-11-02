using System;

namespace QuietHull.Data
{
    /// <summary>
    /// Lane Authority scans at regulated docks. Each scan looks at every contraband item and every unit
    /// with contraband fitted. Anything in plain sight is found at <see cref="OpenDetectionChance"/>;
    /// anything in a hidden compartment only if the compartment fails. Pure.
    /// </summary>
    public static class ContrabandMath
    {
        /// <summary>Chance a scan finds contraband that isn't hidden.</summary>
        public const float OpenDetectionChance = 0.6f;

        public static float DetectionChance(bool hidden, ShuttleModuleDefinition compartment)
        {
            if (!hidden || compartment == null || !compartment.IsHiddenCompartment)
            {
                return OpenDetectionChance;
            }

            return 1f - compartment.ScanEvasion;
        }

        /// <summary>
        /// Chance a scan finds anything: each of <paramref name="openItems"/> in plain sight is checked
        /// separately; the compartment is checked once, and if it fails everything inside is found.
        /// </summary>
        public static float ChanceAnythingFound(int openItems, int hiddenItems, ShuttleModuleDefinition compartment)
        {
            if (openItems < 0 || hiddenItems < 0)
            {
                throw new ArgumentOutOfRangeException(openItems < 0 ? nameof(openItems) : nameof(hiddenItems));
            }

            var missOpen = Math.Pow(1.0 - OpenDetectionChance, openItems);
            var missHidden = hiddenItems > 0 ? 1.0 - DetectionChance(true, compartment) : 1.0;
            return (float)(1.0 - missOpen * missHidden);
        }
    }
}
