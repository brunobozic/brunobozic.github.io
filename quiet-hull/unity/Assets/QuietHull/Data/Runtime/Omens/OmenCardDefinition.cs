using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// One line on an Omen card. <see cref="commonValue"/> is the Common value; rarity scales it by
    /// m (benefit side) or h (handicap side) with <see cref="rule"/>. Percent effects store percent
    /// (25 = 25%); Chance and Reduction effects store fractions (0.3 = 30%).
    /// </summary>
    [Serializable]
    public struct OmenEffect
    {
        public OmenEffectKind kind;
        public OmenSide side;
        public ScalingRule rule;
        public float commonValue;

        public OmenEffect(OmenEffectKind kind, OmenSide side, ScalingRule rule, float commonValue)
        {
            this.kind = kind;
            this.side = side;
            this.rule = rule;
            this.commonValue = commonValue;
        }

        public float ValueAt(Rarity rarity) => RarityMath.Scale(commonValue, rule, RarityMath.Factor(side, rarity));

        /// <summary>The value at a rarity and card level (Level I equals <see cref="ValueAt(Rarity)"/>).</summary>
        public float ValueAt(Rarity rarity, int level) => RarityMath.Scale(commonValue, rule, RarityMath.Factor(side, rarity, level));
    }

    /// <summary>
    /// A tarot (Omen) card. The same asset type covers every family: effects scale with rarity,
    /// Brutality cards add a payout per point of Threat, and Vow cards apply their effects only
    /// when the vow is kept (or their penalties when it's broken).
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Omens/Omen Card", fileName = "omen_")]
    public sealed class OmenCardDefinition : DefinitionBase
    {
        [SerializeField] internal OmenFamily family;
        [Tooltip("Minor Arcana only: the reward stream this card pushes. None = a Major card.")]
        [SerializeField] internal OmenSuit suit = OmenSuit.None;
        [SerializeField] internal Rarity rarity = Rarity.Common;
        [Tooltip("Threat at Common; scaled by h and rounded half away from zero.")]
        [SerializeField] internal int commonThreat;

        [Tooltip("Always active (or, for a Vow, active only while the vow is kept).")]
        [SerializeField] internal OmenEffect[] effects = Array.Empty<OmenEffect>();

        [Header("Brutality")]
        [Tooltip("Pays Threat × 4/6/8/10/12% as Spoils or Bounty, chosen when played.")]
        [SerializeField] internal bool brutalityPayout;

        [Header("Culling")]
        [Tooltip("The alien a Culling card thins out (CulledSpawnReduction).")]
        [SerializeField] internal AlienDefinition culledAlien;

        [Header("Vow")]
        [SerializeField] internal VowCondition vowCondition = VowCondition.None;
        [Tooltip("Applied only when the vow is broken.")]
        [SerializeField] internal OmenEffect[] brokenEffects = Array.Empty<OmenEffect>();

        [Header("Corruption")]
        [Tooltip("What a corrupted copy can become (the Heretic outcome). Without one, Heretic falls back to Doubled.")]
        [SerializeField] internal OmenCardDefinition hereticVariant;
        [Tooltip("True for a Heretic variant: only corruption makes it, it never drops.")]
        [SerializeField] internal bool isHeretic;

        [Header("Text")]
        [SerializeField] internal string epicLine = string.Empty;
        [SerializeField] internal string legendaryTwist = string.Empty;

        public OmenFamily Family => family;
        public OmenSuit Suit => suit;
        public OmenArcana Arcana => suit == OmenSuit.None ? OmenArcana.Major : OmenArcana.Minor;
        public OmenCardDefinition HereticVariant => hereticVariant;
        public bool IsHeretic => isHeretic;
        public Rarity Rarity => rarity;
        public int CommonThreat => commonThreat;
        public int Threat => RarityMath.ScaleThreat(commonThreat, rarity);
        public IReadOnlyList<OmenEffect> Effects => effects ?? Array.Empty<OmenEffect>();
        public bool BrutalityPayout => brutalityPayout;
        public AlienDefinition CulledAlien => culledAlien;
        public bool IsVow => vowCondition != VowCondition.None;
        public VowCondition VowCondition => vowCondition;
        public IReadOnlyList<OmenEffect> BrokenEffects => brokenEffects ?? Array.Empty<OmenEffect>();
        public string EpicLine => epicLine;
        public string LegendaryTwist => legendaryTwist;

        /// <summary>Brutality payout in percent at this card's rarity (0 for other families).</summary>
        public float BrutalityPayoutPercent => brutalityPayout ? Threat * RarityMath.BrutalityRatePercent(rarity) : 0f;

        /// <summary>The scaled value of the first effect of <paramref name="kind"/>, or 0.</summary>
        public float ValueOf(OmenEffectKind kind) => FirstValue(Effects, kind, rarity);

        /// <summary>The scaled value of the first broken-vow effect of <paramref name="kind"/>, or 0.</summary>
        public float BrokenValueOf(OmenEffectKind kind) => FirstValue(BrokenEffects, kind, rarity);

        private static float FirstValue(IReadOnlyList<OmenEffect> list, OmenEffectKind kind, Rarity rarity)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].kind == kind)
                {
                    return list[i].ValueAt(rarity);
                }
            }

            return 0f;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (Effects.Count == 0 && !brutalityPayout)
            {
                report.Error(this, nameof(effects), "a card must do something.");
            }

            foreach (var effect in Effects)
            {
                ValidateEffect(report, nameof(effects), effect);
            }

            if (IsVow)
            {
                if (family != OmenFamily.Vow)
                {
                    report.Error(this, nameof(vowCondition), "only Vow cards have a vow condition.");
                }

                report.NotEmpty(this, nameof(brokenEffects), brokenEffects);
                foreach (var effect in BrokenEffects)
                {
                    ValidateEffect(report, nameof(brokenEffects), effect);
                }
            }
            else if (family == OmenFamily.Vow)
            {
                report.Error(this, nameof(vowCondition), "a Vow card needs a condition.");
            }

            if ((family == OmenFamily.Suit) != (suit != OmenSuit.None))
            {
                report.Error(this, nameof(suit), "Minor Arcana cards (family Suit) need a suit, and only they have one.");
            }

            if (hereticVariant != null)
            {
                if (hereticVariant == this || !hereticVariant.isHeretic)
                {
                    report.Error(this, nameof(hereticVariant), "the Heretic variant must be another card marked isHeretic.");
                }
                else if (hereticVariant.family != family)
                {
                    report.Error(this, nameof(hereticVariant), "the Heretic variant keeps the card's family.");
                }
            }

            if (family == OmenFamily.Culling)
            {
                report.NotNull(this, nameof(culledAlien), culledAlien);
                if (ValueOf(OmenEffectKind.CulledSpawnReduction) <= 0f)
                {
                    report.Error(this, nameof(effects), "a Culling card needs a CulledSpawnReduction effect.");
                }
            }
            else if (culledAlien != null)
            {
                report.Warning(this, nameof(culledAlien), "only Culling cards cull an alien.");
            }

            if (brutalityPayout && family != OmenFamily.Brutality)
            {
                report.Warning(this, nameof(brutalityPayout), "only Brutality cards normally pay per Threat.");
            }

            if (rarity >= Rarity.Legendary && string.IsNullOrWhiteSpace(legendaryTwist))
            {
                report.Warning(this, nameof(legendaryTwist), "a Legendary card should have its twist written.");
            }
        }

        private void ValidateEffect(ValidationReport report, string field, OmenEffect effect)
        {
            if ((effect.rule == ScalingRule.Chance || effect.rule == ScalingRule.Reduction) && (effect.commonValue < 0f || effect.commonValue > 1f))
            {
                report.Error(this, field, $"{effect.kind}: Chance and Reduction values are fractions in [0, 1].");
            }

            if (effect.rule == ScalingRule.Multiplier && effect.commonValue < 1f)
            {
                report.Warning(this, field, $"{effect.kind}: a multiplier below 1 shrinks as rarity rises.");
            }
        }
    }
}
