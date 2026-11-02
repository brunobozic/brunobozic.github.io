using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A permanent specialization a combat unit picks once it has enough experience.
    /// Each grows into one of the four doctrine archetypes.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Specialization", fileName = "spec_")]
    public sealed class SpecializationDefinition : DefinitionBase
    {
        [SerializeField] internal Archetype archetype;
        [Min(0)] [SerializeField] internal int unlockExperience = 50;

        [Tooltip("Replaces the unit's weapon when chosen (null keeps the current one).")]
        [SerializeField] internal ItemDefinition weaponOverride;

        [SerializeField] internal StatModifier[] passives = Array.Empty<StatModifier>();

        [Header("Ability")]
        [SerializeField] internal string abilityId = string.Empty;
        [SerializeField] internal string abilityName = string.Empty;
        [TextArea(2, 5)] [SerializeField] internal string abilityDescription = string.Empty;
        [Min(0.1f)] [SerializeField] internal float abilityCooldownSeconds = 10f;
        [Min(0f)] [SerializeField] internal float abilityDurationSeconds;
        [Min(0f)] [SerializeField] internal float abilityRadius;

        [Header("Design notes")]
        [Tooltip("What it answers, shown in the choice screen.")]
        [SerializeField] internal string strongAgainst = string.Empty;
        [SerializeField] internal string weakAgainst = string.Empty;

        public Archetype Archetype => archetype;
        public int UnlockExperience => unlockExperience;
        public ItemDefinition WeaponOverride => weaponOverride;
        public IReadOnlyList<StatModifier> Passives => passives ?? Array.Empty<StatModifier>();
        public string AbilityId => abilityId;
        public string AbilityName => abilityName;
        public string AbilityDescription => abilityDescription;
        public float AbilityCooldownSeconds => abilityCooldownSeconds;
        public float AbilityDurationSeconds => abilityDurationSeconds;
        public float AbilityRadius => abilityRadius;
        public string StrongAgainst => strongAgainst;
        public string WeakAgainst => weakAgainst;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(abilityId) || string.IsNullOrWhiteSpace(abilityName))
            {
                report.Error(this, nameof(abilityId), "a specialization needs an ability id and name.");
            }

            report.Positive(this, nameof(abilityCooldownSeconds), abilityCooldownSeconds);
            if (Passives.Count == 0)
            {
                report.Warning(this, nameof(passives), "has no passive effects.");
            }

            if (string.IsNullOrWhiteSpace(strongAgainst) || string.IsNullOrWhiteSpace(weakAgainst))
            {
                report.Warning(this, nameof(strongAgainst), "say what it's strong and weak against, for the choice screen.");
            }
        }
    }
}
