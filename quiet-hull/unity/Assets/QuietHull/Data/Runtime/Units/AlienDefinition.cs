using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>An alien type. Values match the enemy and balance tables in the design doc.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Alien", fileName = "alien_")]
    public sealed class AlienDefinition : DefinitionBase
    {
        [Header("Body")]
        [Min(1f)] [SerializeField] internal float maxHealth = 40f;
        [Min(0f)] [SerializeField] internal float armour;
        [Min(0.1f)] [SerializeField] internal float moveSpeed = 4.5f;
        [SerializeField] internal bool usesDucts;

        [Header("Combat")]
        [SerializeField] internal AttackProfile attack = new AttackProfile(AttackKind.Melee, 8f, 1f, 1.2f, 0f);
        [Tooltip("What it goes for, in priority order.")]
        [SerializeField] internal TargetKind[] targetPriority = { TargetKind.NearestUnit };

        [Header("Senses")]
        [SerializeField] internal SenseProfile senses = new SenseProfile(1f, 1f, 6f, 120f, 0f);
        [Tooltip("Radius of the shriek that pulls other aliens in. 0 = cannot signal.")]
        [Min(0f)] [SerializeField] internal float signalRadius;
        [Min(0f)] [SerializeField] internal float signalCooldownSeconds = 6f;
        [Tooltip("Seconds without sound, sight or heat before it gives up the hunt.")]
        [Min(0.5f)] [SerializeField] internal float giveUpSeconds = 10f;

        [Header("Behaviour and economy")]
        [SerializeField] internal AlienBehaviour defaultBehaviour = AlienBehaviour.Patrol;
        [Min(0)] [SerializeField] internal int surgeCost = 1;
        [Min(0)] [SerializeField] internal int chitinBounty = 1;
        [Min(0)] [SerializeField] internal int experience = 1;
        [Header("Death burst (Bloats)")]
        [Min(0f)] [SerializeField] internal float deathBurstDamage;
        [Min(0f)] [SerializeField] internal float deathBurstRadius;
        [Tooltip("Its burst eats doors and barricades.")]
        [SerializeField] internal bool deathBurstEatsDoors;
        [Tooltip("Damage per second to doors and barricades it touches while alive.")]
        [Min(0f)] [SerializeField] internal float doorDamagePerSecond;

        [Tooltip("Highest Heat intensity (0–3) it can stand. Above it, it burns: it flees if it can and dies if it can't.")]
        [Range(0, 3)] [SerializeField] internal int heatTolerance = 1;
        [Tooltip("How much acid blood it sprays when killed, toward an amalgam deposit (0 = none, e.g. synthetics).")]
        [Min(0f)] [SerializeField] internal float amalgamWeight = 1f;

        public float MaxHealth => maxHealth;
        public float Armour => armour;
        public float MoveSpeed => moveSpeed;
        public bool UsesDucts => usesDucts;
        public AttackProfile Attack => attack;
        public IReadOnlyList<TargetKind> TargetPriority => targetPriority ?? Array.Empty<TargetKind>();
        public SenseProfile Senses => senses;
        public SenseFlags SenseFlags => senses.Senses;
        public bool HasHeatVision => (senses.Senses & SenseFlags.Heat) != 0;
        public float SignalRadius => signalRadius;
        public bool CanSignal => signalRadius > 0f;
        public float SignalCooldownSeconds => signalCooldownSeconds;
        public float GiveUpSeconds => giveUpSeconds;
        public AlienBehaviour DefaultBehaviour => defaultBehaviour;
        public int SurgeCost => surgeCost;
        public int ChitinBounty => chitinBounty;
        public int Experience => experience;
        public float AmalgamWeight => amalgamWeight;
        public int HeatTolerance => heatTolerance;
        public float DeathBurstDamage => deathBurstDamage;
        public float DeathBurstRadius => deathBurstRadius;
        public bool DeathBurstEatsDoors => deathBurstEatsDoors;
        public float DoorDamagePerSecond => doorDamagePerSecond;
        public bool BurstsOnDeath => deathBurstDamage > 0f && deathBurstRadius > 0f;

        /// <summary>Damage per second for each Heat level above tolerance.</summary>
        public const float HeatBurnPerLevel = 20f;

        /// <summary>Burn damage per second at a Heat intensity (0 when it can stand it).</summary>
        public float HeatBurnPerSecond(int heatIntensity) => Math.Max(0, heatIntensity - heatTolerance) * HeatBurnPerLevel * ArmourMath.DamageMultiplier(armour);

        /// <summary>Seconds it survives at a Heat intensity; infinity if it can stand it.</summary>
        public float SecondsToBurn(int heatIntensity)
        {
            var burn = HeatBurnPerSecond(heatIntensity);
            return burn > 0f ? maxHealth / burn : float.PositiveInfinity;
        }

        /// <summary>Damage a hit of <paramref name="rawDamage"/> deals after armour.</summary>
        public float DamageTaken(float rawDamage) => rawDamage * ArmourMath.DamageMultiplier(armour);

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.Positive(this, nameof(maxHealth), maxHealth);
            report.Positive(this, nameof(moveSpeed), moveSpeed);
            report.Positive(this, "attack.damage", attack.damage);
            report.Positive(this, "attack.intervalSeconds", attack.intervalSeconds);
            report.Positive(this, "attack.range", attack.range);
            report.NotEmpty(this, nameof(targetPriority), targetPriority);

            if (senses.Senses == SenseFlags.None)
            {
                report.Error(this, nameof(senses), "an alien must have at least one sense.");
            }

            if (CanSignal)
            {
                report.Positive(this, nameof(signalCooldownSeconds), signalCooldownSeconds);
            }

            if ((deathBurstDamage > 0f) != (deathBurstRadius > 0f))
            {
                report.Error(this, nameof(deathBurstRadius), "a death burst needs both damage and radius.");
            }
        }
    }
}
