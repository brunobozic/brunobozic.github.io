using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A gun: the service pistol in the prologue.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Ranged Weapon", fileName = "itm_ranged_")]
    public sealed class RangedWeaponDefinition : ItemDefinition
    {
        [Header("Ranged")]
        [Min(0f)] [SerializeField] internal float damagePerShot = 22f;
        [Min(0.02f)] [SerializeField] internal float fireIntervalSeconds = 0.33f;
        [Min(0.5f)] [SerializeField] internal float range = 15f;
        [Min(1)] [SerializeField] internal int magazineSize = 12;
        [Min(0f)] [SerializeField] internal float reloadSeconds = 1.5f;

        [Tooltip("Fraction a hit slows its target (Controller foam). 0 = none.")]
        [Range(0f, 0.95f)] [SerializeField] internal float slowFraction;
        [Min(0f)] [SerializeField] internal float slowSeconds;

        [Tooltip("Scripted moments (the prologue finale) ignore reserve ammo.")]
        [SerializeField] internal bool unlimitedReserve;

        public override ItemCategory Category => ItemCategory.RangedWeapon;
        public float DamagePerShot => damagePerShot;
        public float FireIntervalSeconds => fireIntervalSeconds;
        public float Range => range;
        public int MagazineSize => magazineSize;
        public float ReloadSeconds => reloadSeconds;
        public bool UnlimitedReserve => unlimitedReserve;
        public float SlowFraction => slowFraction;
        public float SlowSeconds => slowSeconds;

        /// <summary>Sustained DPS including reloads.</summary>
        public float SustainedDamagePerSecond
        {
            get
            {
                var cycleSeconds = magazineSize * fireIntervalSeconds + reloadSeconds;
                return cycleSeconds > 0f ? magazineSize * damagePerShot / cycleSeconds : 0f;
            }
        }

        protected override void ValidateItem(ValidationReport report)
        {
            if (!(damagePerShot > 0f) && !(slowFraction > 0f))
            {
                report.Error(this, nameof(damagePerShot), "a weapon must damage or slow.");
            }

            if (slowFraction > 0f)
            {
                report.Positive(this, nameof(slowSeconds), slowSeconds);
            }

            report.Positive(this, nameof(fireIntervalSeconds), fireIntervalSeconds);
            report.Positive(this, nameof(range), range);
            report.Positive(this, nameof(magazineSize), magazineSize);
            report.NonNegative(this, nameof(reloadSeconds), reloadSeconds);
        }
    }
}
