using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One tier of the player's shop.</summary>
    [Serializable]
    public struct ShopTier
    {
        public string name;
        [Min(0)] public int upgradeCost;
        [Min(1)] public int displaySlots;
        [Min(1)] public int salesPerReturn;
        [Min(0.1f)] public float priceMultiplier;
        [Min(0)] public int rentPerReturn;
        public bool hasBackCounter;
        public bool takesFactionOrders;
        public FactionDefinition requiresFaction;
        public Standing requiresStanding;
    }

    /// <summary>
    /// The player's stall at a hub (Patusan). Sells salvage while the player is away, for more than the
    /// Exchange pays, with rent, a cut to the host faction, theft and inspections.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Hub/Shop", fileName = "shop_")]
    public sealed class ShopDefinition : DefinitionBase
    {
        /// <summary>The Exchange pays this share of an item's value; the shop's base price is value itself.</summary>
        public const float ExchangeShare = 0.6f;
        public const float DemandMultiplier = 1.5f;
        public const float ContrabandMultiplier = 2f;

        [SerializeField] internal FactionDefinition hostFaction;
        [SerializeField] internal ShopTier[] tiers = Array.Empty<ShopTier>();
        [Tooltip("Host faction's cut by standing: Neutral, Friendly, Trusted (Hostile seizes the shop).")]
        [SerializeField] internal float[] cutByStanding = { 0.2f, 0.1f, 0.05f };
        [Range(0f, 1f)] [SerializeField] internal float theftChancePerItem = 0.1f;
        [Range(0f, 1f)] [SerializeField] internal float inspectionChance = 0.1f;
        [Tooltip("Flag set when the shop is first offered.")]
        [SerializeField] internal string unlockFlag = string.Empty;

        public FactionDefinition HostFaction => hostFaction;
        public IReadOnlyList<ShopTier> Tiers => tiers ?? Array.Empty<ShopTier>();
        public float TheftChancePerItem => theftChancePerItem;
        public float InspectionChance => inspectionChance;
        public string UnlockFlag => unlockFlag;

        /// <summary>The host faction's cut at a standing; 1 (everything) when they're Hostile: the shop is seized.</summary>
        public float CutAt(Standing standing)
        {
            if (standing <= Standing.Hostile)
            {
                return 1f;
            }

            var index = Math.Max(0, Math.Min(cutByStanding.Length - 1, (int)standing));
            return cutByStanding[index];
        }

        public bool IsSeized(PlayerStanding standing) => standing != null && standing.Of(hostFaction) <= Standing.Hostile;

        /// <summary>What the player nets for one item sold at <paramref name="tierIndex"/>, given its Exchange value.</summary>
        public float NetSalePrice(int exchangeValue, int tierIndex, bool inDemand, bool contraband, Standing standing)
        {
            var value = exchangeValue / ExchangeShare;
            var price = value * Tiers[tierIndex].priceMultiplier * (inDemand ? DemandMultiplier : 1f) * (contraband ? ContrabandMultiplier : 1f);
            return price * (1f - CutAt(standing));
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(hostFaction), hostFaction);
            report.NotEmpty(this, nameof(tiers), tiers);
            for (var i = 1; i < Tiers.Count; i++)
            {
                if (Tiers[i].salesPerReturn < Tiers[i - 1].salesPerReturn || Tiers[i].priceMultiplier < Tiers[i - 1].priceMultiplier)
                {
                    report.Error(this, nameof(tiers), $"tier {Tiers[i].name} must not be worse than {Tiers[i - 1].name}.");
                }
            }
        }
    }
}
