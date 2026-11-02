using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// The effective numbers of a weapon after mods and unit modifiers. An immutable value:
    /// computed once when a loadout changes, then read by the sim from any thread.
    /// </summary>
    public readonly struct WeaponStats
    {
        public WeaponStats(float damagePerShot, float fireIntervalSeconds, int magazineSize, float reloadSeconds, float accuracy, FireMode fireMode, int burstLength)
        {
            DamagePerShot = damagePerShot;
            FireIntervalSeconds = fireIntervalSeconds;
            MagazineSize = magazineSize;
            ReloadSeconds = reloadSeconds;
            Accuracy = accuracy;
            FireMode = fireMode;
            BurstLength = burstLength;
        }

        public float DamagePerShot { get; }
        public float FireIntervalSeconds { get; }
        public int MagazineSize { get; }
        public float ReloadSeconds { get; }
        public float Accuracy { get; }
        public FireMode FireMode { get; }
        public int BurstLength { get; }

        /// <summary>Damage per second over a full magazine and reload, if every shot hits.</summary>
        public float SustainedDamagePerSecond
        {
            get
            {
                var cycle = MagazineSize * FireIntervalSeconds + ReloadSeconds;
                return cycle > 0f ? MagazineSize * DamagePerShot / cycle : 0f;
            }
        }

        /// <summary>Sustained DPS × accuracy: what actually lands on a moving swarm.</summary>
        public float EffectiveDamagePerSecond => SustainedDamagePerSecond * Accuracy;

        /// <summary>
        /// Applies unit modifiers (augments, specialization passives): FireRate shortens the interval,
        /// ReloadTime scales reloads, Accuracy adds points (capped at 0.95).
        /// </summary>
        public WeaponStats WithUnitModifiers(IReadOnlyList<StatModifier> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return this;
            }

            var fireRate = StatMath.Apply(UnitStat.FireRate, 1f, modifiers);
            var reload = StatMath.Apply(UnitStat.ReloadTime, ReloadSeconds, modifiers);
            var accuracy = Math.Min(0.95f, StatMath.Apply(UnitStat.Accuracy, Accuracy, modifiers));

            return new WeaponStats(
                DamagePerShot,
                fireRate > 0f ? FireIntervalSeconds / fireRate : FireIntervalSeconds,
                MagazineSize,
                Math.Max(0f, reload),
                accuracy,
                FireMode,
                BurstLength);
        }

        public override string ToString() =>
            $"{FireMode} {DamagePerShot:0.#} dmg / {FireIntervalSeconds:0.###} s, mag {MagazineSize}, reload {ReloadSeconds:0.#} s, acc {Accuracy:P0}, eff {EffectiveDamagePerSecond:0.0} dps";
    }
}
