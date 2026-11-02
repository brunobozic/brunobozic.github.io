using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A hand cart that carries cargo. Its wheels are the loudest thing J1M does.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Cart", fileName = "itm_cart_")]
    public sealed class CartDefinition : ItemDefinition
    {
        [Header("Cart")]
        [Min(1)] [SerializeField] internal int capacitySlots = 6;
        [Min(1f)] [SerializeField] internal float maxHealth = 200f;
        [Tooltip("Noise N0 while moving (the wheels).")]
        [Min(0f)] [SerializeField] internal float noiseMoving = 6f;
        [Min(0.1f)] [SerializeField] internal float pushSpeed = 2.2f;

        public override ItemCategory Category => ItemCategory.Vehicle;
        public int CapacitySlots => capacitySlots;
        public float MaxHealth => maxHealth;
        public float NoiseMoving => noiseMoving;
        public float PushSpeed => pushSpeed;

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(capacitySlots), capacitySlots);
            report.Positive(this, nameof(maxHealth), maxHealth);
            report.Positive(this, nameof(pushSpeed), pushSpeed);
        }
    }
}
