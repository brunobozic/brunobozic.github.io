using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// What workers do on their own: automatic salvage every run, scavenging (open rooms, search,
    /// salvage) and sentry tactics (place and restock sentries where the swarm comes from). Pure.
    /// </summary>
    public static class WorkerMath
    {
        /// <summary>Each worker present adds this much automatic salvage.</summary>
        public const float SalvagePerWorker = 0.15f;
        /// <summary>A full crew, every one of them fitted for salvage, adds this on top.</summary>
        public const float FullCrewBonus = 0.25f;

        public const int ScavengeAutonomy = 2;
        public const int SentryTacticsAutonomy = 3;
        /// <summary>Scavengers must be reasonably stealthy: at most this footstep noise...</summary>
        public const float ScavengeMaxNoise = 1.5f;
        /// <summary>...and seen at no more than this share of normal range (−10%).</summary>
        public const float ScavengeMaxVisibility = -0.1f;

        /// <summary>
        /// Scrip of automatic salvage for a run: base × (1 + 0.15 per worker) × (1 + average worker salvage
        /// yield) × full-crew bonus × (1 + card salvage %).
        /// </summary>
        public static int AutoSalvage(int baseScrip, IReadOnlyList<LoadoutProfile> workers, int crewCapacity, float cardSalvagePercent)
        {
            if (baseScrip <= 0)
            {
                return 0;
            }

            var count = workers?.Count ?? 0;
            var yieldSum = 0f;
            var allFitted = count > 0;
            for (var i = 0; i < count; i++)
            {
                var y = StatMath.SumFlat(UnitStat.SalvageYield, workers[i].Modifiers);
                yieldSum += y;
                allFitted &= y > 0f;
            }

            var average = count > 0 ? yieldSum / count : 0f;
            var full = allFitted && crewCapacity > 0 && count >= crewCapacity ? 1f + FullCrewBonus : 1f;
            var total = baseScrip * (1f + SalvagePerWorker * count) * (1f + average) * full * (1f + cardSalvagePercent / 100f);
            return (int)Math.Round(Math.Max(0f, total), MidpointRounding.AwayFromZero);
        }

        /// <summary>A worker's autonomy: modules (rounded down) plus what its firmware allows.</summary>
        public static int Autonomy(UnitDefinition unit, Mark mark, LoadoutProfile profile)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            var fromModules = profile != null ? (int)Math.Floor(StatMath.SumFlat(UnitStat.Autonomy, profile.Modifiers) + 1e-4f) : 0;
            var fromFirmware = unit.MarkCurve != null ? unit.MarkCurve.FirmwareAutonomyAt(mark) : 0;
            return fromModules + fromFirmware;
        }

        /// <summary>Smart and quiet enough to be sent ahead to open rooms, search and salvage on its own.</summary>
        public static bool CanScavenge(UnitDefinition unit, Mark mark, LoadoutProfile profile) =>
            profile != null && Autonomy(unit, mark, profile) >= ScavengeAutonomy &&
            profile.NoiseWalking <= ScavengeMaxNoise && profile.VisibilityPercent <= ScavengeMaxVisibility;

        /// <summary>Smart enough to place sentries where it sees swarms coming from, move them when routes change, and restock them.</summary>
        public static bool CanRunSentryTactics(UnitDefinition unit, Mark mark, LoadoutProfile profile) =>
            Autonomy(unit, mark, profile) >= SentryTacticsAutonomy;
    }
}
