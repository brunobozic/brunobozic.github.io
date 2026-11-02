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

        [Tooltip("Scripted moments (the prologue finale) ignore reserve ammo.")]
        [SerializeField] internal bool unlimitedReserve;

        public override ItemCategory Category => ItemCategory.RangedWeapon;
        public float DamagePerShot => damagePerShot;
        public float FireIntervalSeconds => fireIntervalSeconds;
        public float Range => range;
        public int MagazineSize => magazineSize;
        public float ReloadSeconds => reloadSeconds;
        public bool UnlimitedReserve => unlimitedReserve;

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
            report.Positive(this, nameof(damagePerShot), damagePerShot);
            report.Positive(this, nameof(fireIntervalSeconds), fireIntervalSeconds);
            report.Positive(this, nameof(range), range);
            report.Positive(this, nameof(magazineSize), magazineSize);
            report.NonNegative(this, nameof(reloadSeconds), reloadSeconds);
        }
    }
}
