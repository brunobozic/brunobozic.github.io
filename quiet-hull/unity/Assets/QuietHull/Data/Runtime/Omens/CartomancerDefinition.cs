using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// The Cartomancer's table at Patusan: card levels from duplicates (I–V), ascending a Level V card one
    /// rarity, and Catalyst mutations (Reroll, Deepen, Refine, Corrupt). Replaces combining 5 → 1.
    /// Every operation is a pure function of its inputs (randomness comes in as a roll in [0, 1)), so it is
    /// thread-safe and replayable.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Omens/Cartomancer", fileName = "cartomancer_")]
    public sealed class CartomancerDefinition : DefinitionBase
    {
        [Header("Levels")]
        [Tooltip("Duplicate points to go from Level I→II, II→III, III→IV, IV→V.")]
        [SerializeField] internal int[] copiesPerLevel = { 1, 2, 3, 4 };
        [Tooltip("Scrip fee for each of those level-ups.")]
        [SerializeField] internal int[] levelFees = { 25, 50, 100, 200 };
        [Tooltip("Quiet Choir at Trusted: one copy fewer for each level-up that reaches this level or higher.")]
        [Range(2, 5)] [SerializeField] internal int trustedDiscountFromLevel = 3;

        [Header("Ascending (Level V → next rarity, Level I)")]
        [Tooltip("Scrip fee to ascend from Common, Uncommon, Rare, Epic.")]
        [SerializeField] internal int[] ascendFees = { 50, 150, 400, 1000 };

        [Header("Mutations")]
        [SerializeField] internal OmenHandicapPool[] handicapPools = Array.Empty<OmenHandicapPool>();
        [Tooltip("Corruption weights: Nothing, Ascended, Doubled, Heretic.")]
        [SerializeField] internal float[] corruptionWeights = { 1f, 1f, 1f, 1f };

        public IReadOnlyList<int> CopiesPerLevel => copiesPerLevel ?? Array.Empty<int>();
        public IReadOnlyList<int> LevelFees => levelFees ?? Array.Empty<int>();
        public IReadOnlyList<int> AscendFees => ascendFees ?? Array.Empty<int>();
        public int TrustedDiscountFromLevel => trustedDiscountFromLevel;

        /// <summary>Duplicate points a copy is worth: Common 1, Uncommon 2, Rare 3, Epic 4, Legendary 5.</summary>
        public static int DuplicateWorth(Rarity rarity) => (int)rarity + 1;

        /// <summary>The Catalyst each mutation consumes.</summary>
        public static CatalystKind CatalystFor(CardMutation mutation)
        {
            switch (mutation)
            {
                case CardMutation.Reroll: return CatalystKind.Reroll;
                case CardMutation.Deepen: return CatalystKind.Mastercraft;
                case CardMutation.Refine: return CatalystKind.Refine;
                case CardMutation.Corrupt: return CatalystKind.HiveTainted;
                default: throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
            }
        }

        /// <summary>Duplicate points needed to leave <paramref name="level"/>.</summary>
        public int CopiesToLevelUp(int level, bool quietChoirTrusted)
        {
            if (level < 1 || level >= RarityMath.MaxCardLevel)
            {
                return 0;
            }

            var copies = CopiesPerLevel[level - 1];
            return quietChoirTrusted && level + 1 >= trustedDiscountFromLevel ? Math.Max(1, copies - 1) : copies;
        }

        public int LevelFee(int level) => level >= 1 && level < RarityMath.MaxCardLevel ? LevelFees[level - 1] : 0;

        public int AscendFee(Rarity rarity) => rarity < Rarity.Legendary ? AscendFees[(int)rarity] : 0;

        public IReadOnlyList<OmenEffect> PoolFor(OmenFamily family)
        {
            var pools = handicapPools ?? Array.Empty<OmenHandicapPool>();
            for (var i = 0; i < pools.Length; i++)
            {
                if (pools[i].family == family)
                {
                    return pools[i].handicaps ?? Array.Empty<OmenEffect>();
                }
            }

            return Array.Empty<OmenEffect>();
        }

        /// <summary>Banks a duplicate copy of the same card. Corrupted cards take no more duplicates.</summary>
        public CartomancyResult AddDuplicate(OmenCardInstance card, OmenCardDefinition duplicate, Rarity duplicateRarity)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (duplicate != card.Definition)
            {
                return CartomancyResult.Fail(card, "only a copy of the same card counts.");
            }

            if (card.IsCorrupted)
            {
                return CartomancyResult.Fail(card, "a corrupted card can't grow.");
            }

            return CartomancyResult.Ok(card.WithProgress(checked(card.Progress + DuplicateWorth(duplicateRarity))));
        }

        public CartomancyResult LevelUp(OmenCardInstance card, int scrip, bool quietChoirTrusted)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (card.IsCorrupted)
            {
                return CartomancyResult.Fail(card, "a corrupted card can't be levelled.");
            }

            if (card.Level >= RarityMath.MaxCardLevel)
            {
                return CartomancyResult.Fail(card, "already Level V: ascend it instead.");
            }

            var copies = CopiesToLevelUp(card.Level, quietChoirTrusted);
            if (card.Progress < copies)
            {
                return CartomancyResult.Fail(card, $"needs {copies} duplicate points, has {card.Progress}.");
            }

            var fee = LevelFee(card.Level);
            if (scrip < fee)
            {
                return CartomancyResult.Fail(card, $"costs {fee} Scrip.");
            }

            return CartomancyResult.Ok(card.WithLevel(card.Level + 1, card.Progress - copies), fee);
        }

        /// <summary>Lifts a Level V card one rarity, back to Level I, up to <paramref name="ceiling"/> (the rollout's).</summary>
        public CartomancyResult Ascend(OmenCardInstance card, int scrip, Rarity ceiling)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (card.IsCorrupted)
            {
                return CartomancyResult.Fail(card, "a corrupted card can't ascend.");
            }

            if (card.Level < RarityMath.MaxCardLevel)
            {
                return CartomancyResult.Fail(card, "only a Level V card can ascend.");
            }

            if (card.Rarity >= Rarity.Legendary || card.Rarity + 1 > ceiling)
            {
                return CartomancyResult.Fail(card, $"the table can't make {card.Rarity + 1} yet.");
            }

            var fee = AscendFee(card.Rarity);
            return scrip < fee ? CartomancyResult.Fail(card, $"costs {fee} Scrip.") : CartomancyResult.Ok(card.Ascended(), fee);
        }

        /// <summary>
        /// Applies a Catalyst mutation. <paramref name="roll"/> in [0, 1) picks the pooled handicap (Reroll, Deepen)
        /// or the corruption outcome. Corruption is irreversible: the card can never be levelled, ascended or mutated again.
        /// </summary>
        public CartomancyResult Mutate(OmenCardInstance card, CardMutation mutation, float roll)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (card.IsCorrupted)
            {
                return CartomancyResult.Fail(card, "a corrupted card can't be changed again.");
            }

            roll = roll < 0f ? 0f : roll >= 1f ? 0.999999f : roll;
            switch (mutation)
            {
                case CardMutation.Reroll:
                {
                    var pool = PoolFor(card.Family);
                    return pool.Count == 0
                        ? CartomancyResult.Fail(card, $"no handicap pool for {card.Family}.")
                        : CartomancyResult.Ok(card.WithRerolledHandicap(pool[(int)(roll * pool.Count)]));
                }

                case CardMutation.Deepen:
                {
                    if (card.IsDeepened)
                    {
                        return CartomancyResult.Fail(card, "already deepened.");
                    }

                    var pool = PoolFor(card.Family);
                    return pool.Count == 0
                        ? CartomancyResult.Fail(card, $"no handicap pool for {card.Family}.")
                        : CartomancyResult.Ok(card.WithDeepen(pool[(int)(roll * pool.Count)]));
                }

                case CardMutation.Refine:
                    return card.Level >= RarityMath.MaxCardLevel
                        ? CartomancyResult.Fail(card, "already Level V.")
                        : CartomancyResult.Ok(card.WithLevel(card.Level + 1, card.Progress));

                case CardMutation.Corrupt:
                    return CartomancyResult.Ok(Corrupt(card, CorruptionOutcome(roll)));

                default:
                    throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
            }
        }

        /// <summary>The corruption outcome a roll in [0, 1) lands on, by the weights.</summary>
        public CardCorruption CorruptionOutcome(float roll)
        {
            var weights = corruptionWeights ?? Array.Empty<float>();
            var total = 0f;
            for (var i = 0; i < weights.Length; i++)
            {
                total += Math.Max(0f, weights[i]);
            }

            var target = roll * total;
            var cumulative = 0f;
            for (var i = 0; i < weights.Length; i++)
            {
                cumulative += Math.Max(0f, weights[i]);
                if (target < cumulative)
                {
                    return (CardCorruption)(i + 1);
                }
            }

            return CardCorruption.Nothing;
        }

        private static OmenCardInstance Corrupt(OmenCardInstance card, CardCorruption outcome)
        {
            switch (outcome)
            {
                case CardCorruption.Ascended:
                    return card.Rarity < Rarity.Legendary
                        ? card.WithCorruption(outcome, card.Definition, card.Rarity + 1, false)
                        : card.WithCorruption(outcome, card.Definition, card.Rarity, true);
                case CardCorruption.Heretic:
                    return card.Definition.HereticVariant != null
                        ? card.WithCorruption(outcome, card.Definition.HereticVariant, card.Rarity, false)
                        : card.WithCorruption(CardCorruption.Doubled, card.Definition, card.Rarity, false);
                default:
                    return card.WithCorruption(outcome, card.Definition, card.Rarity, false);
            }
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            var steps = RarityMath.MaxCardLevel - 1;
            if (CopiesPerLevel.Count != steps || LevelFees.Count != steps)
            {
                report.Error(this, nameof(copiesPerLevel), $"copies and fees need {steps} entries (I→II … IV→V).");
            }
            else
            {
                for (var i = 0; i < steps; i++)
                {
                    report.Positive(this, nameof(copiesPerLevel), CopiesPerLevel[i]);
                    report.NonNegative(this, nameof(levelFees), LevelFees[i]);
                    if (i > 0 && (CopiesPerLevel[i] < CopiesPerLevel[i - 1] || LevelFees[i] < LevelFees[i - 1]))
                    {
                        report.Error(this, nameof(copiesPerLevel), "each level costs at least as much as the one before.");
                    }
                }
            }

            if (AscendFees.Count != (int)Rarity.Legendary)
            {
                report.Error(this, nameof(ascendFees), "one ascend fee per rarity below Legendary.");
            }

            if ((corruptionWeights?.Length ?? 0) != 4)
            {
                report.Error(this, nameof(corruptionWeights), "four weights: Nothing, Ascended, Doubled, Heretic.");
            }

            foreach (var pool in handicapPools ?? Array.Empty<OmenHandicapPool>())
            {
                foreach (var effect in pool.handicaps ?? Array.Empty<OmenEffect>())
                {
                    if (effect.side != OmenSide.Handicap)
                    {
                        report.Error(this, nameof(handicapPools), $"{pool.family}: a handicap pool holds handicaps only ({effect.kind}).");
                    }
                }
            }
        }
    }
}
