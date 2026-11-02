using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A thrown item such as the stun grenade.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Throwable", fileName = "itm_throwable_")]
    public sealed class ThrowableDefinition : ItemDefinition
    {
        [Header("Throwable")]
        [Min(0.5f)] [SerializeField] internal float throwRange = 10f;
        [Min(0.1f)] [SerializeField] internal float effectRadius = 4f;
        [Min(0f)] [SerializeField] internal float fuseSeconds = 1.5f;
        [Min(0f)] [SerializeField] internal float stunSeconds = 4f;
        [Min(0f)] [SerializeField] internal float damage;
        [Min(1)] [SerializeField] internal int maxStack = 3;

        public override ItemCategory Category => ItemCategory.Throwable;
        public float ThrowRange => throwRange;
        public float EffectRadius => effectRadius;
        public float FuseSeconds => fuseSeconds;
        public float StunSeconds => stunSeconds;
        public float Damage => damage;
        public int MaxStack => maxStack;

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(throwRange), throwRange);
            report.Positive(this, nameof(effectRadius), effectRadius);
            if (!(stunSeconds > 0f) && !(damage > 0f))
            {
                report.Error(this, nameof(stunSeconds), "a throwable must stun or damage.");
            }
        }
    }
}
