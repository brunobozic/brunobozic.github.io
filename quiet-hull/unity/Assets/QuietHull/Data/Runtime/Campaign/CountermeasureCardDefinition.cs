using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A commander countermeasure card: Marlow's own verb, played from the shuttle.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Campaign/Countermeasure Card", fileName = "card_")]
    public sealed class CountermeasureCardDefinition : DefinitionBase
    {
        [SerializeField] internal CountermeasureEffect effect;
        [Tooltip("Shuttle power the card costs.")]
        [Min(0)] [SerializeField] internal int powerCost = 2;
        [Min(0f)] [SerializeField] internal float cooldownSeconds = 30f;
        [Min(0f)] [SerializeField] internal float durationSeconds;
        [Tooltip("Noise N0 of the effect (a vent roars).")]
        [Min(0f)] [SerializeField] internal float noise;
        [Tooltip("Damage per second to aliens caught in the effect (a vent), 0 = none.")]
        [Min(0f)] [SerializeField] internal float damagePerSecond;

        public CountermeasureEffect Effect => effect;
        public int PowerCost => powerCost;
        public float CooldownSeconds => cooldownSeconds;
        public float DurationSeconds => durationSeconds;
        public float Noise => noise;
        public float DamagePerSecond => damagePerSecond;

        protected override void ValidateDefinition(ValidationReport report) => report.Positive(this, nameof(cooldownSeconds), cooldownSeconds);
    }
}
