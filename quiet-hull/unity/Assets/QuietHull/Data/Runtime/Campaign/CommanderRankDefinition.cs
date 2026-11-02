using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>Marlow's rank: total XP needed for each rank, and a skill point per rank.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Campaign/Commander Ranks", fileName = "ranks_")]
    public sealed class CommanderRankDefinition : DefinitionBase
    {
        [Tooltip("Total XP to reach rank 2, 3, 4, ... (rank 1 is 0).")]
        [SerializeField] internal int[] xpForRank = Array.Empty<int>();
        [Min(0)] [SerializeField] internal int skillPointsPerRank = 1;

        public int MaxRank => (xpForRank?.Length ?? 0) + 1;
        public int SkillPointsPerRank => skillPointsPerRank;

        public int RankAt(int totalXp)
        {
            var rank = 1;
            foreach (var threshold in xpForRank ?? Array.Empty<int>())
            {
                if (totalXp < threshold)
                {
                    break;
                }

                rank++;
            }

            return rank;
        }

        public int SkillPointsAt(int totalXp) => (RankAt(totalXp) - 1) * skillPointsPerRank;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotEmpty(this, nameof(xpForRank), xpForRank);
            var previous = 0;
            foreach (var threshold in xpForRank ?? Array.Empty<int>())
            {
                if (threshold <= previous)
                {
                    report.Error(this, nameof(xpForRank), "thresholds must rise.");
                }

                previous = threshold;
            }
        }
    }
}
