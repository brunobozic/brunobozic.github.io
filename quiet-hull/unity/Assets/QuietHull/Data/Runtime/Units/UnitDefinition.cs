using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A player-controlled unit: J1M the worker android, or Marlow in the prologue finale.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Unit", fileName = "unit_")]
    public sealed class UnitDefinition : DefinitionBase
    {
        [Header("Durability")]
        [Min(1f)] [SerializeField] internal float maxHealth = 300f;
        [Min(0f)] [SerializeField] internal float armour = 10f;

        [Tooltip("False in the prologue: at 0 health the unit is Wrecked, then reboots.")]
        [SerializeField] internal bool canBeDestroyed = true;
        [Min(0f)] [SerializeField] internal float wreckedSeconds = 8f;
        [Range(0.01f, 1f)] [SerializeField] internal float rebootHealthFraction = 0.3f;

        [Header("Movement")]
        [Min(0.1f)] [SerializeField] internal float moveSpeed = 3f;

        [Header("Signature")]
        [Tooltip("Noise N0 while walking.")]
        [Min(0f)] [SerializeField] internal float noiseWalking = 2f;
        [Range(0f, 10f)] [SerializeField] internal float heatIdle = 2f;
        [Range(0f, 10f)] [SerializeField] internal float heatAfterExertion = 5f;
        [Min(0f)] [SerializeField] internal float exertionCooldownSeconds = 6f;
        [Tooltip("Shoulder lamp range in metres; 0 = no lamp. A lit lamp is a bright light source for alien vision.")]
        [Min(0f)] [SerializeField] internal float lampRange;

        [Header("Capabilities")]
        [Tooltip("Light items carried by hand. Heavy cargo needs a cart.")]
        [Min(0)] [SerializeField] internal int handCarrySlots = 1;
        [SerializeField] internal UnitAbility abilities = UnitAbility.Move | UnitAbility.Interact;
        [SerializeField] internal ItemDefinition[] startingItems = Array.Empty<ItemDefinition>();

        [Header("Role")]
        [Tooltip("DefendOnly: workers defend themselves and company assets and can't be ordered to attack. Combat: military.")]
        [SerializeField] internal CombatRole combatRole = CombatRole.DefendOnly;
        [Tooltip("Defend-only units retaliate for attacks on themselves or on company assets within this radius.")]
        [Min(0f)] [SerializeField] internal float defendRadius = 6f;
        [Tooltip("Unarmed or tool damage per second when defending (weapons override it).")]
        [Min(0f)] [SerializeField] internal float defendDamagePerSecond = 6f;
        [Min(0f)] [SerializeField] internal float workSpeed = 1f;

        [Header("Charge (stamina)")]
        [Tooltip("Workers run on a battery; leave capacity at 0 for units that don't track charge.")]
        [SerializeField] internal ChargeProfile charge;

        [Header("Frame")]
        [Tooltip("Augment weight the frame carries before it slows down and gets louder.")]
        [Min(0f)] [SerializeField] internal float augmentCapacityKg = 40f;
        [Tooltip("Built-in power for augments; a Heart (power core) adds more.")]
        [Min(0f)] [SerializeField] internal float basePowerOutputWatts = 200f;
        [Min(0)] [SerializeField] internal int priceScrip;
        [Tooltip("How it improves with its own mark; empty = Mk I only.")]
        [SerializeField] internal UnitMarkCurveDefinition markCurve;

        [Header("Growth")]
        [Tooltip("Upgrade slots this body accepts. List a slot twice for two sockets (workers have two Hardening sockets).")]
        [SerializeField] internal AugmentSlot[] augmentSlots = Array.Empty<AugmentSlot>();
        [Tooltip("Specializations offered once the unit has enough experience.")]
        [SerializeField] internal SpecializationDefinition[] specializationOptions = Array.Empty<SpecializationDefinition>();

        public float MaxHealth => maxHealth;
        public float Armour => armour;
        public bool CanBeDestroyed => canBeDestroyed;
        public float WreckedSeconds => wreckedSeconds;
        public float RebootHealthFraction => rebootHealthFraction;
        public float MoveSpeed => moveSpeed;
        public float NoiseWalking => noiseWalking;
        public float HeatIdle => heatIdle;
        public float HeatAfterExertion => heatAfterExertion;
        public float ExertionCooldownSeconds => exertionCooldownSeconds;
        public float LampRange => lampRange;
        public bool HasLamp => lampRange > 0f;
        public int HandCarrySlots => handCarrySlots;
        public UnitAbility Abilities => abilities;
        public IReadOnlyList<ItemDefinition> StartingItems => startingItems ?? Array.Empty<ItemDefinition>();
        public IReadOnlyList<AugmentSlot> AugmentSlots => augmentSlots ?? Array.Empty<AugmentSlot>();
        public ChargeProfile Charge => charge;
        public bool UsesCharge => charge.capacity > 0f;
        public CombatRole CombatRole => combatRole;
        public bool CanBeOrderedToAttack => combatRole == CombatRole.Combat;
        public float DefendRadius => defendRadius;
        public float DefendDamagePerSecond => defendDamagePerSecond;
        public float WorkSpeed => workSpeed;
        public float AugmentCapacityKg => augmentCapacityKg;
        public float BasePowerOutputWatts => basePowerOutputWatts;
        public int PriceScrip => priceScrip;
        public UnitMarkCurveDefinition MarkCurve => markCurve;
        public Mark MaxMark => markCurve != null ? markCurve.MaxMark : Mark.MkI;

        /// <summary>How many sockets of <paramref name="slot"/> this body has.</summary>
        public int SlotCount(AugmentSlot slot)
        {
            var count = 0;
            foreach (var candidate in AugmentSlots)
            {
                if (candidate == slot)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>This unit's own stats at <paramref name="mark"/>, before augments. Pure.</summary>
        public UnitStatsAtMark StatsAt(Mark mark)
        {
            if (mark < Mark.MkI || mark > MaxMark)
            {
                throw new ArgumentOutOfRangeException(nameof(mark), mark, $"{id} goes up to {MaxMark}.");
            }

            var steps = (int)mark - 1;
            var curve = markCurve;
            if (curve == null || steps == 0)
            {
                return new UnitStatsAtMark(mark, maxHealth, armour, moveSpeed, workSpeed, defendDamagePerSecond, noiseWalking, heatIdle,
                    basePowerOutputWatts, handCarrySlots, priceScrip, curve != null ? curve.FirmwareAt(mark) : FirmwareGeneration.Gen3);
            }

            var extraSlots = curve.ExtraCarrySlotEvery > 0 ? steps / curve.ExtraCarrySlotEvery : 0;
            return new UnitStatsAtMark(
                mark,
                maxHealth * (1f + curve.HealthPerMark * steps),
                armour + curve.ArmourPerMark * steps,
                moveSpeed + curve.MoveSpeedPerMark * steps,
                workSpeed * (1f + curve.WorkSpeedPerMark * steps),
                defendDamagePerSecond * (1f + curve.DefendDamagePerMark * steps),
                Math.Max(curve.MinimumNoise, noiseWalking + curve.NoisePerMark * steps),
                heatIdle,
                basePowerOutputWatts + curve.PowerOutputPerMark * steps,
                handCarrySlots + extraSlots,
                (int)Math.Round(priceScrip * Math.Pow(curve.PriceFactorPerMark, steps), MidpointRounding.AwayFromZero),
                curve.FirmwareAt(mark));
        }
        public IReadOnlyList<SpecializationDefinition> SpecializationOptions => specializationOptions ?? Array.Empty<SpecializationDefinition>();

        public bool Accepts(AugmentDefinition augment) => augment != null && SlotCount(augment.Slot) > 0;

        public bool Can(UnitAbility ability) => (abilities & ability) == ability;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.Positive(this, nameof(maxHealth), maxHealth);
            report.Positive(this, nameof(moveSpeed), moveSpeed);
            if (!canBeDestroyed)
            {
                report.Positive(this, nameof(wreckedSeconds), wreckedSeconds);
            }

            if (heatAfterExertion < heatIdle)
            {
                report.Warning(this, nameof(heatAfterExertion), "is lower than the idle heat.");
            }

            if (!Can(UnitAbility.Move))
            {
                report.Warning(this, nameof(abilities), "unit cannot move.");
            }

            foreach (var item in StartingItems)
            {
                report.NotNull(this, nameof(startingItems), item);
            }

            var hasBody = false;
            var hasWork = false;
            foreach (var slot in AugmentSlots)
            {
                hasBody |= slot < AugmentSlot.Frame;
                hasWork |= slot >= AugmentSlot.Frame;
            }

            if (hasBody && hasWork)
            {
                report.Error(this, nameof(augmentSlots), "a unit takes either synth body slots or worker work slots, not both.");
            }

            if (combatRole == CombatRole.DefendOnly && hasBody)
            {
                report.Warning(this, nameof(combatRole), "a synth with body slots is normally a Combat unit.");
            }

            foreach (var option in SpecializationOptions)
            {
                report.NotNull(this, nameof(specializationOptions), option);
            }

            if (UsesCharge)
            {
                if (charge.drainPerSecond == null || charge.drainPerSecond.Length != ChargeProfile.ActivityCount)
                {
                    report.Error(this, nameof(charge), $"needs a drain rate for each of the {ChargeProfile.ActivityCount} activities.");
                }

                report.Positive(this, "charge.rechargePerSecond", charge.rechargePerSecond);
            }
        }
    }
}
