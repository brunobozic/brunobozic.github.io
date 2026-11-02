using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A melee weapon such as the shock prod.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Melee Weapon", fileName = "itm_melee_")]
    public sealed class MeleeWeaponDefinition : ItemDefinition
    {
        [Header("Melee")]
        [Min(0f)] [SerializeField] internal float damage = 20f;
        [Min(0.05f)] [SerializeField] internal float attackIntervalSeconds = 1.2f;
        [Min(0.1f)] [SerializeField] internal float range = 1.8f;

        [Tooltip("Every Nth consecutive hit on the same target stuns it. 0 = never.")]
        [Min(0)] [SerializeField] internal int stunEveryNthHit = 3;
        [Min(0f)] [SerializeField] internal float stunSeconds = 1.5f;

        public override ItemCategory Category => ItemCategory.MeleeWeapon;
        public float Damage => damage;
        public float AttackIntervalSeconds => attackIntervalSeconds;
        public float Range => range;
        public int StunEveryNthHit => stunEveryNthHit;
        public float StunSeconds => stunSeconds;
        public float DamagePerSecond => damage / attackIntervalSeconds;

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(damage), damage);
            report.Positive(this, nameof(attackIntervalSeconds), attackIntervalSeconds);
            report.Positive(this, nameof(range), range);
            if (stunEveryNthHit > 0)
            {
                report.Positive(this, nameof(stunSeconds), stunSeconds);
            }
        }
    }
}
