using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A kit that converts one weapon (the micro-conversion kit for the standard military handgun:
    /// automatic fire, recoil damping, extended caseless magazines). Crafted from a blueprint.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Weapon Mod", fileName = "itm_mod_")]
    public sealed class WeaponModDefinition : ItemDefinition
    {
        [Header("Fits")]
        [SerializeField] internal RangedWeaponDefinition fitsWeapon;

        [Header("Changes")]
        [SerializeField] internal FireMode fireMode = FireMode.Auto;
        [Min(1)] [SerializeField] internal int burstLength = 1;
        [Tooltip("Multiplies the time between shots (0.45 = more than twice as fast).")]
        [Range(0.05f, 2f)] [SerializeField] internal float fireIntervalMultiplier = 1f;
        [Tooltip("Multiplies damage per shot (lighter caseless loads).")]
        [Range(0.1f, 2f)] [SerializeField] internal float damageMultiplier = 1f;
        [Tooltip("Magazine size after the mod (0 = unchanged).")]
        [Min(0)] [SerializeField] internal int magazineSize;
        [Range(0.1f, 3f)] [SerializeField] internal float reloadMultiplier = 1f;
        [Tooltip("Added to accuracy (recoil damping offsets the spray).")]
        [Range(-0.5f, 0.5f)] [SerializeField] internal float accuracyAdd;
        [Tooltip("Spare extended magazines that come with the kit (flavour and reload animation).")]
        [Min(0)] [SerializeField] internal int spareMagazines;

        public override ItemCategory Category => ItemCategory.WeaponMod;
        public RangedWeaponDefinition FitsWeapon => fitsWeapon;
        public FireMode FireMode => fireMode;
        public int BurstLength => burstLength;
        public float FireIntervalMultiplier => fireIntervalMultiplier;
        public float DamageMultiplier => damageMultiplier;
        public int MagazineSize => magazineSize;
        public float ReloadMultiplier => reloadMultiplier;
        public float AccuracyAdd => accuracyAdd;
        public int SpareMagazines => spareMagazines;

        /// <summary>The weapon's stats with this kit fitted. Pure.</summary>
        public WeaponStats Apply(WeaponStats baseStats) => new WeaponStats(
            baseStats.DamagePerShot * damageMultiplier,
            baseStats.FireIntervalSeconds * fireIntervalMultiplier,
            magazineSize > 0 ? magazineSize : baseStats.MagazineSize,
            baseStats.ReloadSeconds * reloadMultiplier,
            Mathf.Clamp(baseStats.Accuracy + accuracyAdd, 0.05f, 0.95f),
            fireMode,
            fireMode == FireMode.Burst ? burstLength : 1);

        protected override void ValidateItem(ValidationReport report)
        {
            report.NotNull(this, nameof(fitsWeapon), fitsWeapon);
            if (fireMode == FireMode.Burst && burstLength < 2)
            {
                report.Error(this, nameof(burstLength), "a burst mod needs at least 2 rounds per burst.");
            }
        }
    }
}
