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

        [Header("Growth")]
        [Tooltip("Augment slots this body accepts (empty for workers).")]
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
        public IReadOnlyList<SpecializationDefinition> SpecializationOptions => specializationOptions ?? Array.Empty<SpecializationDefinition>();

        public bool Accepts(AugmentDefinition augment)
        {
            if (augment == null)
            {
                return false;
            }

            foreach (var slot in AugmentSlots)
            {
                if (slot == augment.Slot)
                {
                    return true;
                }
            }

            return false;
        }

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

            var slots = new HashSet<AugmentSlot>();
            foreach (var slot in AugmentSlots)
            {
                if (!slots.Add(slot))
                {
                    report.Error(this, nameof(augmentSlots), $"{slot} is listed twice.");
                }
            }

            foreach (var option in SpecializationOptions)
            {
                report.NotNull(this, nameof(specializationOptions), option);
            }
        }
    }
}
