using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// A card the player owns: its definition plus everything the Cartomancer did to it (rarity, level,
    /// duplicate progress, mutations, corruption). Immutable: every change returns a new instance, so a
    /// deck can be read from any thread and a failed operation never leaves a half-changed card.
    /// </summary>
    public sealed class OmenCardInstance
    {
        /// <summary>Deepen: the benefit grows by this factor.</summary>
        public const float DeepenBenefitFactor = 1.25f;

        /// <summary>Deepen: the card's Threat grows by this much.</summary>
        public const int DeepenThreat = 2;

        /// <summary>Doubled corruption: both sides grow by this factor.</summary>
        public const float DoubledFactor = 1.5f;

        private readonly OmenEffect _deepenHandicap;
        private readonly OmenEffect _rerolledHandicap;

        public OmenCardInstance(OmenCardDefinition definition)
            : this(definition, definition?.Rarity ?? Rarity.Common, 1, 0, CardCorruption.None, false, default, false, default, false)
        {
        }

        private OmenCardInstance(OmenCardDefinition definition, Rarity rarity, int level, int progress, CardCorruption corruption,
            bool deepened, OmenEffect deepenHandicap, bool rerolled, OmenEffect rerolledHandicap, bool extraTwist)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (level < 1 || level > RarityMath.MaxCardLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, $"card levels run 1..{RarityMath.MaxCardLevel}.");
            }

            Rarity = rarity;
            Level = level;
            Progress = Math.Max(0, progress);
            Corruption = corruption;
            IsDeepened = deepened;
            _deepenHandicap = deepenHandicap;
            HasRerolledHandicap = rerolled;
            _rerolledHandicap = rerolledHandicap;
            HasExtraTwist = extraTwist;
        }

        public OmenCardDefinition Definition { get; }
        public Rarity Rarity { get; }
        public int Level { get; }

        /// <summary>Duplicate points banked toward the next level (an Uncommon copy is worth 2, a Rare 3, ...).</summary>
        public int Progress { get; }

        public CardCorruption Corruption { get; }
        public bool IsCorrupted => Corruption != CardCorruption.None;
        public bool IsDeepened { get; }
        public bool HasRerolledHandicap { get; }

        /// <summary>A corrupted Legendary that rolled Ascended: it carries a second Legendary twist.</summary>
        public bool HasExtraTwist { get; }

        public OmenArcana Arcana => Definition.Arcana;
        public OmenFamily Family => Definition.Family;
        public OmenSuit Suit => Definition.Suit;

        /// <summary>Card Threat: the definition's Threat at this rarity and level, plus 2 when Deepened.</summary>
        public int Threat => RarityMath.ScaleThreat(Definition.CommonThreat, Rarity, Level) + (IsDeepened ? DeepenThreat : 0);

        /// <summary>The Threat the Brutality payout counts: doubled by the Doubled corruption.</summary>
        public int PayoutThreat => Corruption == CardCorruption.Doubled ? Threat * 2 : Threat;

        public float BrutalityPayoutPercent => Definition.BrutalityPayout ? PayoutThreat * RarityMath.BrutalityRatePercent(Rarity) : 0f;

        /// <summary>The effects in play: the definition's, with the handicap swapped if rerolled, plus the Deepen handicap.</summary>
        public IReadOnlyList<OmenEffect> Effects
        {
            get
            {
                var source = Definition.Effects;
                var result = new List<OmenEffect>(source.Count + 2);
                var swapped = false;
                for (var i = 0; i < source.Count; i++)
                {
                    if (HasRerolledHandicap && source[i].side == OmenSide.Handicap)
                    {
                        if (!swapped)
                        {
                            result.Add(_rerolledHandicap);
                            swapped = true;
                        }

                        continue;
                    }

                    result.Add(source[i]);
                }

                if (HasRerolledHandicap && !swapped)
                {
                    result.Add(_rerolledHandicap);
                }

                if (IsDeepened)
                {
                    result.Add(_deepenHandicap);
                }

                return result;
            }
        }

        /// <summary>
        /// The summed value of every effect of <paramref name="kind"/> at this rarity and level, with Deepen
        /// (benefit ×1.25) and the Doubled corruption (both sides ×1.5) applied. Percent and Count values scale;
        /// Chance and Reduction values stay fractions and are capped at 1.
        /// </summary>
        public float ValueOf(OmenEffectKind kind)
        {
            var total = 0f;
            var effects = Effects;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect.kind != kind)
                {
                    continue;
                }

                var value = effect.ValueAt(Rarity, Level);
                var factor = 1f;
                if (IsDeepened && effect.side == OmenSide.Benefit)
                {
                    factor *= DeepenBenefitFactor;
                }

                if (Corruption == CardCorruption.Doubled)
                {
                    factor *= DoubledFactor;
                }

                total += Amplify(value, effect.rule, factor);
            }

            return total;
        }

        private static float Amplify(float value, ScalingRule rule, float factor)
        {
            switch (rule)
            {
                case ScalingRule.None:
                    return value;
                case ScalingRule.Multiplier:
                    return 1f + (value - 1f) * factor;
                case ScalingRule.Chance:
                case ScalingRule.Reduction:
                    return Math.Min(1f, value * factor);
                default:
                    return value * factor;
            }
        }

        internal OmenCardInstance WithProgress(int progress) =>
            new OmenCardInstance(Definition, Rarity, Level, progress, Corruption, IsDeepened, _deepenHandicap, HasRerolledHandicap, _rerolledHandicap, HasExtraTwist);

        internal OmenCardInstance WithLevel(int level, int progress) =>
            new OmenCardInstance(Definition, Rarity, level, progress, Corruption, IsDeepened, _deepenHandicap, HasRerolledHandicap, _rerolledHandicap, HasExtraTwist);

        internal OmenCardInstance Ascended() =>
            new OmenCardInstance(Definition, Rarity + 1, 1, 0, Corruption, IsDeepened, _deepenHandicap, HasRerolledHandicap, _rerolledHandicap, HasExtraTwist);

        internal OmenCardInstance WithRerolledHandicap(OmenEffect handicap) =>
            new OmenCardInstance(Definition, Rarity, Level, Progress, Corruption, IsDeepened, _deepenHandicap, true, handicap, HasExtraTwist);

        internal OmenCardInstance WithDeepen(OmenEffect handicap) =>
            new OmenCardInstance(Definition, Rarity, Level, Progress, Corruption, true, handicap, HasRerolledHandicap, _rerolledHandicap, HasExtraTwist);

        internal OmenCardInstance WithCorruption(CardCorruption corruption, OmenCardDefinition definition, Rarity rarity, bool extraTwist) =>
            new OmenCardInstance(definition, rarity, Level, Progress, corruption, IsDeepened, _deepenHandicap, HasRerolledHandicap, _rerolledHandicap, extraTwist);

        public override string ToString() =>
            $"{Definition.DisplayName} [{Rarity} L{Level}{(IsDeepened ? " deepened" : string.Empty)}{(IsCorrupted ? $" corrupted:{Corruption}" : string.Empty)}]";
    }
}
