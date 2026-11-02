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

        [Header("Gate")]
        [Tooltip("The choice stays locked until the player owns an item with this tag (e.g. long_rifle). Empty = always open.")]
        [SerializeField] internal string requiresEquipmentTag = string.Empty;
        [Tooltip("Shown on the locked choice, e.g. \"Needs a long rifle\".")]
        [SerializeField] internal string lockedReason = string.Empty;

        [SerializeField] internal StatModifier[] passives = Array.Empty<StatModifier>();

        [Header("Matching module")]
        [Tooltip("Worker trades: the module this trade upgrades instead of duplicating (Miner → mining beam).")]
        [SerializeField] internal AugmentDefinition upgradesModule;
        [Tooltip("Marks the matching module gains while the trade is active (capped at the module's maximum).")]
        [Range(0, 4)] [SerializeField] internal int moduleMarkBonus = 2;

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
        public string RequiresEquipmentTag => requiresEquipmentTag;
        public string LockedReason => lockedReason;

        public IReadOnlyList<StatModifier> Passives => passives ?? Array.Empty<StatModifier>();
        public AugmentDefinition UpgradesModule => upgradesModule;
        public int ModuleMarkBonus => moduleMarkBonus;

        /// <summary>The mark <paramref name="module"/> works at with this trade: +bonus for the matching module, capped.</summary>
        public Mark EffectiveMark(AugmentDefinition module, Mark fitted)
        {
            if (module == null || module != upgradesModule || moduleMarkBonus <= 0)
            {
                return fitted;
            }

            return (Mark)Math.Min((int)fitted + moduleMarkBonus, (int)module.MaxMark);
        }

        /// <summary>
        /// The passives that apply. With the matching module fitted, passives on stats the module already
        /// covers drop out (the module's marks carry them instead), so trade and module never stack twice.
        /// </summary>
        public IReadOnlyList<StatModifier> PassivesWith(bool matchingModuleFitted)
        {
            if (!matchingModuleFitted || upgradesModule == null)
            {
                return Passives;
            }

            var covered = new HashSet<UnitStat>();
            foreach (var modifier in upgradesModule.Modifiers)
            {
                covered.Add(modifier.stat);
            }

            var result = new List<StatModifier>(Passives.Count);
            foreach (var passive in Passives)
            {
                if (!covered.Contains(passive.stat))
                {
                    result.Add(passive);
                }
            }

            return result;
        }
        public string AbilityId => abilityId;
        public string AbilityName => abilityName;
        public string AbilityDescription => abilityDescription;
        public float AbilityCooldownSeconds => abilityCooldownSeconds;
        public float AbilityDurationSeconds => abilityDurationSeconds;
        public float AbilityRadius => abilityRadius;
        public string StrongAgainst => strongAgainst;
        public string WeakAgainst => weakAgainst;

        /// <summary>
        /// Whether the choice is open given what the player owns. Locked choices are shown but can't be
        /// picked, so no one specializes into a role they have no gear for.
        /// </summary>
        public bool IsAvailable(IEnumerable<ItemDefinition> owned)
        {
            if (string.IsNullOrEmpty(requiresEquipmentTag))
            {
                return true;
            }

            if (owned == null)
            {
                return false;
            }

            foreach (var item in owned)
            {
                if (item == null)
                {
                    continue;
                }

                foreach (var tag in item.Tags)
                {
                    if (string.Equals(tag, requiresEquipmentTag, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

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

            if (!string.IsNullOrEmpty(requiresEquipmentTag) && string.IsNullOrWhiteSpace(lockedReason))
            {
                report.Error(this, nameof(lockedReason), "a gated specialization must say what it needs.");
            }

            if (string.IsNullOrWhiteSpace(strongAgainst) || string.IsNullOrWhiteSpace(weakAgainst))
            {
                report.Warning(this, nameof(strongAgainst), "say what it's strong and weak against, for the choice screen.");
            }
        }
    }
}
