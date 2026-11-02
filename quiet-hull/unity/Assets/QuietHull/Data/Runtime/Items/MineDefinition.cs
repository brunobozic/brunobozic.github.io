using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A mine a worker (a Sapper fastest) carries and plants. Mines are quiet until they go off, and
    /// most are loud when they do, so a minefield is also an alarm. Damage, stun, slow and lure effects
    /// can combine.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Mine", fileName = "itm_mine_")]
    public sealed class MineDefinition : ItemDefinition
    {
        [Header("Mine")]
        [SerializeField] internal MineTrigger trigger = MineTrigger.Proximity;
        [Min(0f)] [SerializeField] internal float triggerRadius = 2f;
        [Tooltip("Directional only: cone width in degrees.")]
        [Range(0f, 180f)] [SerializeField] internal float coneDegrees;
        [Tooltip("Seconds after planting before it can go off.")]
        [Min(0f)] [SerializeField] internal float armSeconds = 3f;
        [Tooltip("Seconds to plant (a Sapper deploys 50% faster).")]
        [Min(0.1f)] [SerializeField] internal float plantSeconds = 4f;
        [Tooltip("Proximity mines can hit your own units.")]
        [SerializeField] internal bool friendlyFire = true;
        [Tooltip("Hand-carry slots one mine takes.")]
        [Min(1)] [SerializeField] internal int carrySlots = 1;
        [Min(1)] [SerializeField] internal int maxStack = 4;

        [Header("Effect")]
        [Min(0f)] [SerializeField] internal float effectRadius = 3f;
        [Min(0f)] [SerializeField] internal float damage;
        [Min(0f)] [SerializeField] internal float stunSeconds;
        [Range(0f, 0.95f)] [SerializeField] internal float slowFraction;
        [Min(0f)] [SerializeField] internal float slowSeconds;
        [Tooltip("Lures: noise N0 emitted for the duration instead of a blast.")]
        [Min(0f)] [SerializeField] internal float lureNoise;
        [Min(0f)] [SerializeField] internal float lureSeconds;
        [Tooltip("Noise N0 of the detonation itself.")]
        [Min(0f)] [SerializeField] internal float detonationNoise = 12f;
        [Tooltip("Hulk condition intensity added in the blast area (cryo mines add Cold), 0 = none.")]
        [Range(0, 3)] [SerializeField] internal int coldIntensity;

        public override ItemCategory Category => ItemCategory.Mine;
        public MineTrigger Trigger => trigger;
        public float TriggerRadius => triggerRadius;
        public float ConeDegrees => coneDegrees;
        public float ArmSeconds => armSeconds;
        public float PlantSeconds => plantSeconds;
        public bool FriendlyFire => friendlyFire;
        public int CarrySlots => carrySlots;
        public int MaxStack => maxStack;
        public float EffectRadius => effectRadius;
        public float Damage => damage;
        public float StunSeconds => stunSeconds;
        public float SlowFraction => slowFraction;
        public float SlowSeconds => slowSeconds;
        public float LureNoise => lureNoise;
        public float LureSeconds => lureSeconds;
        public float DetonationNoise => detonationNoise;
        public int ColdIntensity => coldIntensity;
        public bool IsLure => lureNoise > 0f;

        /// <summary>Whether this mine goes off for a unit of the player's (synths and workers have no acid blood).</summary>
        public bool TriggersOnPlayerUnits => friendlyFire && trigger != MineTrigger.AcidSense && trigger != MineTrigger.Remote && trigger != MineTrigger.Timed;

        /// <summary>Plant time with the planter's DeploySpeed modifiers.</summary>
        public float PlantSecondsWith(System.Collections.Generic.IReadOnlyList<StatModifier> modifiers)
        {
            var speed = StatMath.Apply(UnitStat.DeploySpeed, 1f, modifiers);
            return speed > 0f ? plantSeconds / speed : plantSeconds;
        }

        /// <summary>Whether the blast kills an alien outright (after its armour).</summary>
        public bool Kills(AlienDefinition alien) => alien != null && alien.DamageTaken(damage) >= alien.MaxHealth;

        protected override void ValidateItem(ValidationReport report)
        {
            var doesSomething = damage > 0f || stunSeconds > 0f || slowFraction > 0f || lureNoise > 0f;
            if (!doesSomething)
            {
                report.Error(this, nameof(damage), "a mine must damage, stun, slow or lure.");
            }

            if (trigger == MineTrigger.Directional && !(coneDegrees > 0f))
            {
                report.Error(this, nameof(coneDegrees), "a directional mine needs a cone.");
            }

            if (trigger != MineTrigger.Timed && trigger != MineTrigger.Remote)
            {
                report.Positive(this, nameof(triggerRadius), triggerRadius);
            }

            if (lureNoise > 0f)
            {
                report.Positive(this, nameof(lureSeconds), lureSeconds);
            }

            if (slowFraction > 0f)
            {
                report.Positive(this, nameof(slowSeconds), slowSeconds);
            }
        }
    }
}
