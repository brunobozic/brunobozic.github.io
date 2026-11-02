using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A gun: the standard military handgun, the long rifle, the foam projector.
    /// Unit weapons don't use crafted ammo (only sentries do); magazines and reloads still matter.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Ranged Weapon", fileName = "itm_ranged_")]
    public sealed class RangedWeaponDefinition : ItemDefinition
    {
        [Header("Ranged")]
        [Min(0f)] [SerializeField] internal float damagePerShot = 22f;
        [Min(0.02f)] [SerializeField] internal float fireIntervalSeconds = 0.33f;
        [Min(0.5f)] [SerializeField] internal float range = 15f;
        [Min(1)] [SerializeField] internal int magazineSize = 12;
        [Min(0f)] [SerializeField] internal float reloadSeconds = 1.5f;
        [SerializeField] internal FireMode fireMode = FireMode.SemiAuto;
        [Tooltip("Rounds per burst (Burst mode only).")]
        [Min(1)] [SerializeField] internal int burstLength = 1;
        [Tooltip("Expected hit rate against a moving alien at typical range (0–1).")]
        [Range(0.05f, 1f)] [SerializeField] internal float accuracy = 0.55f;

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
        public FireMode FireMode => fireMode;
        public int BurstLength => burstLength;
        public float Accuracy => accuracy;
        public bool UnlimitedReserve => unlimitedReserve;
        public float SlowFraction => slowFraction;
        public float SlowSeconds => slowSeconds;

        /// <summary>The weapon's own numbers, before mods and unit modifiers.</summary>
        public WeaponStats BaseStats => new WeaponStats(damagePerShot, fireIntervalSeconds, magazineSize, reloadSeconds, accuracy, fireMode, burstLength);

        /// <summary>Sustained DPS including reloads, if every shot hits.</summary>
        public float SustainedDamagePerSecond => BaseStats.SustainedDamagePerSecond;

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
            report.InRange(this, nameof(accuracy), accuracy, 0.05f, 1f);

            if (fireMode == FireMode.Burst && burstLength < 2)
            {
                report.Error(this, nameof(burstLength), "a burst weapon needs at least 2 rounds per burst.");
            }
        }
    }
}
