using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// One augment family (a synth body part or a worker module). The asset holds the Mk I values;
    /// <see cref="AugmentMath"/> scales them to any mark up to <see cref="MaxMark"/>. An installed
    /// augment is a family plus a mark (<see cref="InstalledAugment"/>).
    /// Heavy parts hit harder but weigh, draw power, make noise and heat, and draw aliens;
    /// fine and shadow parts trade strength for silence.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Augment", fileName = "aug_")]
    public sealed class AugmentDefinition : ItemDefinition
    {
        [Header("Augment")]
        [SerializeField] internal AugmentSlot slot;
        [SerializeField] internal AugmentKind kind = AugmentKind.Cyber;
        [SerializeField] internal AugmentLine line = AugmentLine.Standard;
        [SerializeField] internal Legality legality = Legality.Legal;
        [Tooltip("Highest mark this family comes in (8 for synth parts, 12 for worker modules).")]
        [SerializeField] internal Mark maxMark = Mark.MkVIII;

        [Tooltip("Mk I effects.")]
        [SerializeField] internal StatModifier[] modifiers = Array.Empty<StatModifier>();

        [Header("Cost of carrying it (Mk I)")]
        [Min(0f)] [SerializeField] internal float weightKg = 5f;
        [Min(0f)] [SerializeField] internal float powerDrawWatts = 40f;
        [Tooltip("Power cores (Heart) supply power instead of drawing it.")]
        [Min(0f)] [SerializeField] internal float powerOutputWatts;

        [Header("Signature (Mk I; negative suppresses)")]
        [SerializeField] internal float noise;
        [SerializeField] internal float heat;
        [SerializeField] internal float vibration;
        [Tooltip("Change to how much aliens prefer this unit as a target (0.1 = +10%).")]
        [SerializeField] internal float aggro;

        [Header("Market")]
        [Tooltip("Mk I price; 0 = not sold, crafted only.")]
        [Min(0)] [SerializeField] internal int priceScrip = 450;

        public override ItemCategory Category => ItemCategory.Augment;
        public AugmentSlot Slot => slot;
        public AugmentKind Kind => kind;
        public AugmentLine Line => line;
        public Legality Legality => legality;
        public Mark MaxMark => maxMark;
        public IReadOnlyList<StatModifier> Modifiers => modifiers ?? Array.Empty<StatModifier>();
        public float WeightKg => weightKg;
        public float PowerDrawWatts => powerDrawWatts;
        public float PowerOutputWatts => powerOutputWatts;
        public float Noise => noise;
        public float Heat => heat;
        public float Vibration => vibration;
        public float Aggro => aggro;
        public int PriceScrip => priceScrip;
        public bool SoldAtClinic => priceScrip > 0 && legality == Legality.Legal;
        public bool IsWorkerModule => slot >= AugmentSlot.Frame;

        public bool HasMark(Mark mark) => mark >= Mark.MkI && mark <= maxMark;

        public float WeightAt(Mark mark) => weightKg * AugmentMath.WeightFactor(line, mark);
        public float PowerDrawAt(Mark mark) => powerDrawWatts * AugmentMath.PowerFactor(line, mark);
        public float PowerOutputAt(Mark mark) => powerOutputWatts * AugmentMath.PowerFactor(line, mark);
        public float NoiseAt(Mark mark) => noise * AugmentMath.SignatureFactor(line, mark);
        public float HeatAt(Mark mark) => heat * AugmentMath.SignatureFactor(line, mark);
        public float VibrationAt(Mark mark) => vibration * AugmentMath.SignatureFactor(line, mark);
        public float AggroAt(Mark mark) => aggro * AugmentMath.SignatureFactor(line, mark);
        public int PriceAt(Mark mark) => (int)Math.Round(priceScrip * AugmentMath.PriceFactor(mark), MidpointRounding.AwayFromZero);
        public int RequiredFabricatorTierAt(Mark mark) => AugmentMath.RequiredFabricatorTier(mark);

        /// <summary>Appends the modifiers at <paramref name="mark"/> to <paramref name="into"/> (no other allocation).</summary>
        public void AddModifiersAt(Mark mark, List<StatModifier> into)
        {
            if (into == null)
            {
                throw new ArgumentNullException(nameof(into));
            }

            foreach (var modifier in Modifiers)
            {
                into.Add(AugmentMath.Scale(modifier, mark));
            }
        }

        public StatModifier[] ModifiersAt(Mark mark)
        {
            var list = new List<StatModifier>(Modifiers.Count);
            AddModifiersAt(mark, list);
            return list.ToArray();
        }

        protected override void ValidateItem(ValidationReport report)
        {
            report.NotEmpty(this, nameof(modifiers), modifiers);
            var seen = new HashSet<UnitStat>();
            foreach (var modifier in Modifiers)
            {
                if (!seen.Add(modifier.stat))
                {
                    report.Warning(this, nameof(modifiers), $"{modifier.stat} is modified twice; merge the entries.");
                }

                if (modifier.amount == 0f)
                {
                    report.Warning(this, nameof(modifiers), $"{modifier.stat} has a zero amount.");
                }
            }

            var cap = IsWorkerModule ? AugmentMath.MaxWorkerModuleMark : AugmentMath.MaxSynthAugmentMark;
            if ((int)maxMark > cap)
            {
                report.Error(this, nameof(maxMark), $"can't exceed Mk {cap} for this slot.");
            }

            if (slot == AugmentSlot.Heart && !(powerOutputWatts > 0f))
            {
                report.Error(this, nameof(powerOutputWatts), "a Heart (power core) must supply power.");
            }

            if (line == AugmentLine.Shadow && legality != Legality.Contraband)
            {
                report.Error(this, nameof(legality), "Shadow parts are contraband.");
            }

            if (line == AugmentLine.Shadow && (noise > 0f || heat > 0f || aggro > 0f))
            {
                report.Error(this, nameof(noise), "Shadow parts must not add noise, heat or aggro.");
            }

            if (line == AugmentLine.Assault && aggro <= 0f)
            {
                report.Warning(this, nameof(aggro), "Assault parts normally draw aliens (aggro > 0).");
            }
        }
    }

    /// <summary>An augment family fitted at a mark. Immutable.</summary>
    public readonly struct InstalledAugment
    {
        public InstalledAugment(AugmentDefinition definition, Mark mark)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (!definition.HasMark(mark))
            {
                throw new ArgumentOutOfRangeException(nameof(mark), mark, $"{definition.Id} only goes up to {definition.MaxMark}.");
            }

            Mark = mark;
        }

        public AugmentDefinition Definition { get; }
        public Mark Mark { get; }

        public override string ToString() => $"{Definition.Id} {Mark}";
    }
}
