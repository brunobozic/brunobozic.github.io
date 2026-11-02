using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// A unit's totals with its augments fitted: weight, power, signature, aggro, speed and the
    /// summed stat modifiers. Computed once when the loadout changes; immutable afterwards, so the
    /// sim can read it from any thread.
    /// </summary>
    public sealed class LoadoutProfile
    {
        // Penalties from the design doc.
        public const float SpeedLossPerOverweightKg = 0.01f;   // −1% move speed per kg over capacity
        public const float NoisePerOverweightKg = 0.1f;        // +1 noise per 10 kg over capacity
        public const float HeatPerOverdrawWatt = 1f / 50f;     // +1 heat per 50 W over output
        public const float CooldownPerOverdrawWatt = 0.1f / 50f; // +10% cooldowns per 50 W over output
        public const float MinimumSpeedFactor = 0.4f;

        internal LoadoutProfile(
            float augmentWeightKg, float overweightKg, float powerOutputWatts, float powerDrawWatts,
            float moveSpeed, float noiseWalking, float heatIdle, float vibration, float aggroPercent,
            float visibilityPercent, float cooldownPenaltyPercent, int contrabandCount, int totalPriceScrip,
            IReadOnlyList<StatModifier> modifiers, IReadOnlyList<string> problems)
        {
            AugmentWeightKg = augmentWeightKg;
            OverweightKg = overweightKg;
            PowerOutputWatts = powerOutputWatts;
            PowerDrawWatts = powerDrawWatts;
            MoveSpeed = moveSpeed;
            NoiseWalking = noiseWalking;
            HeatIdle = heatIdle;
            Vibration = vibration;
            AggroPercent = aggroPercent;
            VisibilityPercent = visibilityPercent;
            CooldownPenaltyPercent = cooldownPenaltyPercent;
            ContrabandCount = contrabandCount;
            TotalPriceScrip = totalPriceScrip;
            Modifiers = modifiers;
            Problems = problems;
        }

        public float AugmentWeightKg { get; }
        public float OverweightKg { get; }
        public float PowerOutputWatts { get; }
        public float PowerDrawWatts { get; }
        public float OverdrawWatts => Math.Max(0f, PowerDrawWatts - PowerOutputWatts);
        public float MoveSpeed { get; }
        public float NoiseWalking { get; }
        public float HeatIdle { get; }
        public float Vibration { get; }
        /// <summary>How much more (or less) aliens prefer this unit as a target, e.g. 0.6 = +60%.</summary>
        public float AggroPercent { get; }
        /// <summary>Change to the range alien eyes see this unit at, e.g. −0.55 = seen at 45% range.</summary>
        public float VisibilityPercent { get; }
        public float CooldownPenaltyPercent { get; }

        /// <summary>
        /// What the player sees: three signature stats. Noise is the louder of footsteps and vibration;
        /// Heat and Aggro as they are. Visibility (seen range) shows as a fourth, smaller readout.
        /// </summary>
        public SignatureSummary Signature => new SignatureSummary(Math.Max(NoiseWalking, Vibration), HeatIdle, AggroPercent, VisibilityPercent);
        public int ContrabandCount { get; }
        public int TotalPriceScrip { get; }
        public IReadOnlyList<StatModifier> Modifiers { get; }

        /// <summary>Why the loadout is invalid (wrong slot, too many in a slot, mark out of range). Empty when valid.</summary>
        public IReadOnlyList<string> Problems { get; }
        public bool IsValid => Problems.Count == 0;

        public float Stat(UnitStat stat, float baseValue) => StatMath.Apply(stat, baseValue, Modifiers);

        /// <summary>Totals a unit at a mark with its augments. Pure; allocates only the result.</summary>
        public static LoadoutProfile Compute(UnitDefinition unit, Mark unitMark, IReadOnlyList<InstalledAugment> augments)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            augments ??= Array.Empty<InstalledAugment>();
            var stats = unit.StatsAt(unitMark);
            var modifiers = new List<StatModifier>(augments.Count * 3);
            var problems = new List<string>();
            var used = new Dictionary<AugmentSlot, int>();

            float weight = 0f, draw = 0f, output = stats.PowerOutputWatts, noise = 0f, heat = 0f, vibration = 0f, aggro = 0f;
            int contraband = 0, price = 0;

            foreach (var installed in augments)
            {
                var augment = installed.Definition;
                var slot = augment.Slot;
                used.TryGetValue(slot, out var count);
                used[slot] = count + 1;

                if (count + 1 > unit.SlotCount(slot))
                {
                    problems.Add($"{augment.Id}: no free {slot} slot on {unit.Id}.");
                    continue;
                }

                weight += augment.WeightAt(installed.Mark);
                draw += augment.PowerDrawAt(installed.Mark);
                output += augment.PowerOutputAt(installed.Mark);
                noise += augment.NoiseAt(installed.Mark);
                heat += augment.HeatAt(installed.Mark);
                vibration += augment.VibrationAt(installed.Mark);
                aggro += augment.AggroAt(installed.Mark);
                price += augment.PriceAt(installed.Mark);
                if (augment.Legality == Legality.Contraband)
                {
                    contraband++;
                }

                augment.AddModifiersAt(installed.Mark, modifiers);
            }

            var capacity = StatMath.Apply(UnitStat.CarryCapacityKg, unit.AugmentCapacityKg, modifiers);
            var overweight = Math.Max(0f, weight - capacity);
            var overdraw = Math.Max(0f, draw - output);

            var speedFactor = Math.Max(MinimumSpeedFactor, 1f - overweight * SpeedLossPerOverweightKg);
            var moveSpeed = StatMath.Apply(UnitStat.MoveSpeed, stats.MoveSpeed, modifiers) * speedFactor;

            var noiseWalking = Math.Max(0f, StatMath.Apply(UnitStat.NoiseWalking, stats.NoiseWalking + noise + overweight * NoisePerOverweightKg, modifiers));
            var heatIdle = Math.Max(0f, StatMath.Apply(UnitStat.HeatSignature, stats.HeatIdle + heat + overdraw * HeatPerOverdrawWatt, modifiers));
            var vib = Math.Max(0f, StatMath.Apply(UnitStat.VibrationSignature, vibration, modifiers));
            var aggroTotal = aggro + StatMath.SumFlat(UnitStat.Aggro, modifiers);
            var visibility = StatMath.SumFlat(UnitStat.Visibility, modifiers);

            return new LoadoutProfile(
                weight, overweight, output, draw, moveSpeed, noiseWalking, heatIdle, vib, aggroTotal,
                Math.Max(-0.9f, visibility), overdraw * CooldownPerOverdrawWatt, contraband, price,
                modifiers, problems);
        }
    }
}
