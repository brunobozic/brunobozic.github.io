using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A faction the player has standing with (Hollow Moon, Lane Authority, Drifters' Union, Quiet Choir, Vashti-Kell).</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Factions/Faction", fileName = "fac_")]
    public sealed class FactionDefinition : DefinitionBase
    {
        [SerializeField] internal Standing startingStanding = Standing.Neutral;
        [Tooltip("What they deal in, shown on the faction screen.")]
        [SerializeField] internal string dealsIn = string.Empty;

        public Standing StartingStanding => startingStanding;
        public string DealsIn => dealsIn;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(dealsIn))
            {
                report.Warning(this, nameof(dealsIn), "is empty.");
            }
        }
    }
}
