using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A scripted group of alien spawns (used by guided missions, not procedural surges).</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Scripting/Wave", fileName = "wave_")]
    public sealed class WaveDefinition : DefinitionBase
    {
        [SerializeField] internal WaveSpawn[] spawns = Array.Empty<WaveSpawn>();

        public IReadOnlyList<WaveSpawn> Spawns => spawns ?? Array.Empty<WaveSpawn>();

        public int TotalCount
        {
            get
            {
                var total = 0;
                foreach (var spawn in Spawns)
                {
                    total += spawn.count;
                }

                return total;
            }
        }

        /// <summary>Seconds from the wave start until its last alien spawns.</summary>
        public float DurationSeconds
        {
            get
            {
                var last = 0f;
                foreach (var spawn in Spawns)
                {
                    last = Mathf.Max(last, spawn.LastSpawnSeconds);
                }

                return last;
            }
        }

        public int CountOf(AlienDefinition alien)
        {
            var total = 0;
            foreach (var spawn in Spawns)
            {
                if (spawn.alien == alien)
                {
                    total += spawn.count;
                }
            }

            return total;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotEmpty(this, nameof(spawns), spawns);
            foreach (var spawn in Spawns)
            {
                report.NotNull(this, $"{nameof(spawns)}.alien", spawn.alien);
                report.Positive(this, $"{nameof(spawns)}.count", spawn.count);
                if (string.IsNullOrWhiteSpace(spawn.spawnPointId))
                {
                    report.Error(this, $"{nameof(spawns)}.spawnPointId", "is empty.");
                }

                if (spawn.count > 1 && !(spawn.intervalSeconds > 0f))
                {
                    report.Warning(this, $"{nameof(spawns)}.intervalSeconds", "several aliens spawn on the same frame.");
                }
            }
        }
    }
}
