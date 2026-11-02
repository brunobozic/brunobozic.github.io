using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One section of hull that the belt wears away when it faces into it.</summary>
    [Serializable]
    public struct ErodingSection
    {
        public string sectionId;
        [Tooltip("The facing at which this section points into the belt.")]
        public int exposedAtFacing;
        [Min(1f)] public float integrity;
        public ErosionOutcome outcome;
        [Tooltip("Room the outcome affects (the room losing its radiation, the passage opened or blocked). Empty for a breach.")]
        public string outcomeTargetRoom;

        public ErodingSection(string sectionId, int exposedAtFacing, float integrity, ErosionOutcome outcome, string outcomeTargetRoom)
        {
            this.sectionId = sectionId;
            this.exposedAtFacing = exposedAtFacing;
            this.integrity = integrity;
            this.outcome = outcome;
            this.outcomeTargetRoom = outcomeTargetRoom;
        }
    }

    /// <summary>
    /// A hulk drifting through an asteroid belt. The face pointing into the belt takes impacts; each
    /// exposed section wears down until it fails. The attitude thrusters that turn the hulk toward or
    /// away from the sun also choose which face the belt grinds: one console, two consequences. Used to
    /// vent a section full of aliens, or to strip a radiation source away so the player can go deeper.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Belt Erosion", fileName = "belt_")]
    public sealed class BeltErosionDefinition : DefinitionBase
    {
        [Tooltip("The hulk's sun exposure: its facings are the same facings.")]
        [SerializeField] internal SunExposureDefinition sunExposure;
        [Min(0f)] [SerializeField] internal float impactsPerMinute = 6f;
        [Min(0f)] [SerializeField] internal float damagePerImpact = 10f;
        [Tooltip("Noise N0 of an impact, heard through the whole section (masks nearby noise for a moment).")]
        [Min(0f)] [SerializeField] internal float impactNoise = 9f;
        [SerializeField] internal ErodingSection[] sections = Array.Empty<ErodingSection>();

        public SunExposureDefinition SunExposure => sunExposure;
        public float ImpactsPerMinute => impactsPerMinute;
        public float DamagePerImpact => damagePerImpact;
        public float ImpactNoise => impactNoise;
        public IReadOnlyList<ErodingSection> Sections => sections ?? Array.Empty<ErodingSection>();

        public float ErosionPerSecond => impactsPerMinute / 60f * damagePerImpact;

        /// <summary>Seconds until the section fails if the hulk holds <paramref name="facing"/>; infinity if it isn't exposed.</summary>
        public float SecondsToFail(string sectionId, int facing, float integrityLeft = -1f)
        {
            var section = Find(sectionId);
            var facings = sunExposure != null ? sunExposure.Facings : 1;
            var wrapped = ((facing % facings) + facings) % facings;
            if (section.exposedAtFacing != wrapped || ErosionPerSecond <= 0f)
            {
                return float.PositiveInfinity;
            }

            var left = integrityLeft >= 0f ? integrityLeft : section.integrity;
            return left / ErosionPerSecond;
        }

        public ErodingSection Find(string sectionId)
        {
            foreach (var section in Sections)
            {
                if (string.Equals(section.sectionId, sectionId, StringComparison.Ordinal))
                {
                    return section;
                }
            }

            throw new KeyNotFoundException($"{id} has no section '{sectionId}'.");
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(sunExposure), sunExposure);
            report.Positive(this, nameof(impactsPerMinute), impactsPerMinute);
            report.Positive(this, nameof(damagePerImpact), damagePerImpact);

            var facings = sunExposure != null ? sunExposure.Facings : int.MaxValue;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var section in Sections)
            {
                if (string.IsNullOrWhiteSpace(section.sectionId) || !ids.Add(section.sectionId))
                {
                    report.Error(this, nameof(sections), $"section id '{section.sectionId}' is empty or duplicated.");
                }

                if (section.exposedAtFacing < 0 || section.exposedAtFacing >= facings)
                {
                    report.Error(this, nameof(sections), $"'{section.sectionId}' faces the belt at an unknown facing {section.exposedAtFacing}.");
                }

                if (section.outcome != ErosionOutcome.Breach && string.IsNullOrWhiteSpace(section.outcomeTargetRoom))
                {
                    report.Error(this, nameof(sections), $"'{section.sectionId}' ({section.outcome}) needs a target room.");
                }
            }
        }
    }
}
