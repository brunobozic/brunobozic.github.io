using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// Which Omen card a Requisition card drop becomes. Without Catalysts every card in the drop pool (cards
    /// already met) is equally likely. A Beacon and the Echo first claim a share for their cards (together at
    /// most <see cref="MaxTargetedShare"/>); a Sigil then steers part of the rest to its family or suit; what is
    /// left stays uniform. A target that isn't in the pool claims nothing, so drops still only come from cards
    /// already met. Pure; thread-safe.
    /// </summary>
    public static class CardDropMath
    {
        /// <summary>Every slotted card claims this share of card drops on its own: playing a card grows it.</summary>
        public const float BuiltInEchoShare = 0.10f;

        /// <summary>Beacon and Echo together never claim more than this; there is always a chance of something else.</summary>
        public const float MaxTargetedShare = 0.9f;

        private static readonly float[] BeaconShares = { 0.20f, 0.30f, 0.45f, 0.60f, 0.80f };
        private static readonly float[] SigilShares = { 0.35f, 0.50f, 0.65f, 0.80f, 1.00f };
        private static readonly float[] EchoShares = { 0.10f, 0.15f, 0.20f, 0.30f, 0.40f };

        /// <summary>Share of card drops a Beacon turns into its card: 20 / 30 / 45 / 60 / 80%.</summary>
        public static float BeaconShare(Rarity rarity) => BeaconShares[(int)rarity];

        /// <summary>Share of the remaining drops a Sigil steers to its family or suit: 35 / 50 / 65 / 80 / 100%.</summary>
        public static float SigilShare(Rarity rarity) => SigilShares[(int)rarity];

        /// <summary>Share each slotted card claims with an Echo: 10 / 15 / 20 / 30 / 40%.</summary>
        public static float EchoShare(Rarity rarity) => EchoShares[(int)rarity];

        /// <summary>The chance of each card in <paramref name="pool"/>, in pool order; the chances sum to 1 (0 for an empty pool).</summary>
        public static List<KeyValuePair<OmenCardDefinition, float>> Chances(IReadOnlyList<OmenCardDefinition> pool, CardTargeting targeting)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool));
            }

            targeting = targeting ?? CardTargeting.None;
            var result = new List<KeyValuePair<OmenCardDefinition, float>>(pool.Count);
            if (pool.Count == 0)
            {
                return result;
            }

            var inPool = new HashSet<OmenCardDefinition>(pool);
            var claims = new Dictionary<OmenCardDefinition, float>();
            var claimed = 0f;

            void Claim(OmenCardDefinition card, float share)
            {
                if (card == null || !inPool.Contains(card) || share <= 0f)
                {
                    return;
                }

                claims.TryGetValue(card, out var current);
                claims[card] = current + share;
                claimed += share;
            }

            if (targeting.HasBeacon)
            {
                Claim(targeting.BeaconCard, BeaconShare(targeting.BeaconRarity));
            }

            var perSlotted = BuiltInEchoShare + (targeting.HasEcho ? EchoShare(targeting.EchoRarity) : 0f);
            foreach (var card in targeting.EchoCards)
            {
                Claim(card, perSlotted);
            }

            // Scale the targeted claims down together if they would crowd out everything else.
            var scale = claimed > MaxTargetedShare ? MaxTargetedShare / claimed : 1f;
            var rest = 1f - claimed * scale;

            var sigilCount = 0;
            if (targeting.HasSigil)
            {
                for (var i = 0; i < pool.Count; i++)
                {
                    if (targeting.SigilMatches(pool[i]))
                    {
                        sigilCount++;
                    }
                }
            }

            var sigilPart = sigilCount > 0 ? rest * SigilShare(targeting.SigilRarity) : 0f;
            var uniformPart = (rest - sigilPart) / pool.Count;
            var sigilEach = sigilCount > 0 ? sigilPart / sigilCount : 0f;

            for (var i = 0; i < pool.Count; i++)
            {
                var card = pool[i];
                claims.TryGetValue(card, out var claim);
                var chance = claim * scale + uniformPart + (targeting.SigilMatches(card) ? sigilEach : 0f);
                result.Add(new KeyValuePair<OmenCardDefinition, float>(card, chance));
            }

            return result;
        }

        /// <summary>The card a drop becomes for a roll in [0, 1); null for an empty pool.</summary>
        public static OmenCardDefinition Resolve(IReadOnlyList<OmenCardDefinition> pool, CardTargeting targeting, float roll)
        {
            var chances = Chances(pool, targeting);
            if (chances.Count == 0)
            {
                return null;
            }

            roll = roll < 0f ? 0f : roll >= 1f ? 0.999999f : roll;
            var cumulative = 0f;
            for (var i = 0; i < chances.Count; i++)
            {
                cumulative += chances[i].Value;
                if (roll < cumulative)
                {
                    return chances[i].Key;
                }
            }

            return chances[chances.Count - 1].Key;
        }
    }
}
