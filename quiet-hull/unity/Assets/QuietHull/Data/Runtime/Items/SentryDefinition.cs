using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// An automated sentry gun. Combat numbers here feed the balance runner; the tuning
    /// target for the prologue is that each sentry ends the stream with 10–20% ammo left.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Sentry", fileName = "itm_sentry_")]
    public sealed class SentryDefinition : ItemDefinition
    {
        [Header("Sentry")]
        [Min(1f)] [SerializeField] internal float maxHealth = 250f;
        [Min(0f)] [SerializeField] internal float armour = 20f;
        [Min(0f)] [SerializeField] internal float damagePerRound = 9f;
        [Min(0.1f)] [SerializeField] internal float roundsPerSecond = 8f;
        [Range(0.05f, 1f)] [SerializeField] internal float expectedHitRate = 0.6f;
        [Min(1f)] [SerializeField] internal float range = 18f;
        [Range(10f, 360f)] [SerializeField] internal float arcDegrees = 90f;
        [Min(1)] [SerializeField] internal int ammoCapacity = 1000;
        [Min(0f)] [SerializeField] internal float deploySeconds = 3f;
        [Min(0f)] [SerializeField] internal float pickupSeconds = 5f;

        public override ItemCategory Category => ItemCategory.Deployable;
        public float MaxHealth => maxHealth;
        public float Armour => armour;
        public float DamagePerRound => damagePerRound;
        public float RoundsPerSecond => roundsPerSecond;
        public float ExpectedHitRate => expectedHitRate;
        public float Range => range;
        public float ArcDegrees => arcDegrees;
        public int AmmoCapacity => ammoCapacity;
        public float DeploySeconds => deploySeconds;
        public float PickupSeconds => pickupSeconds;

        /// <summary>Raw damage per second if every round hits.</summary>
        public float PeakDamagePerSecond => damagePerRound * roundsPerSecond;

        /// <summary>Seconds of continuous fire from a full load.</summary>
        public float SecondsOfFire => ammoCapacity / roundsPerSecond;

        /// <summary>Expected rounds to kill a target, using the design's armour rule 100 / (100 + A).</summary>
        public float ExpectedRoundsToKill(float targetHealth, float targetArmour)
        {
            if (targetHealth <= 0f)
            {
                return 0f;
            }

            var damagePerHit = damagePerRound * ArmourMath.DamageMultiplier(targetArmour);
            var hitsNeeded = Mathf.Ceil(targetHealth / damagePerHit);
            return hitsNeeded / expectedHitRate;
        }

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(maxHealth), maxHealth);
            report.Positive(this, nameof(damagePerRound), damagePerRound);
            report.Positive(this, nameof(roundsPerSecond), roundsPerSecond);
            report.InRange(this, nameof(expectedHitRate), expectedHitRate, 0.05f, 1f);
            report.Positive(this, nameof(ammoCapacity), ammoCapacity);
        }
    }
}
