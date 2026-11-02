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

        [Header("Capabilities")]
        [Tooltip("Light items carried by hand. Heavy cargo needs a cart.")]
        [Min(0)] [SerializeField] internal int handCarrySlots = 1;
        [SerializeField] internal UnitAbility abilities = UnitAbility.Move | UnitAbility.Interact;
        [SerializeField] internal ItemDefinition[] startingItems = Array.Empty<ItemDefinition>();

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
        public int HandCarrySlots => handCarrySlots;
        public UnitAbility Abilities => abilities;
        public IReadOnlyList<ItemDefinition> StartingItems => startingItems ?? Array.Empty<ItemDefinition>();

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
        }
    }
}
