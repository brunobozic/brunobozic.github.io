using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One option of a story choice and everything it changes.</summary>
    [Serializable]
    public struct StoryOption
    {
        public string optionId;
        public string label;
        [TextArea(2, 5)] public string consequence;
        public StandingChange[] standingChanges;
        public LicenceDefinition[] licencesGranted;
        public int scrip;
        [Tooltip("Campaign flags set (e.g. ledger_kept) for later story beats.")]
        public string[] flags;
    }

    /// <summary>A story decision with lasting consequences (the Tremolino's ledger).</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Story/Story Choice", fileName = "choice_")]
    public sealed class StoryChoiceDefinition : DefinitionBase
    {
        [TextArea(3, 8)] [SerializeField] internal string prompt = string.Empty;
        [SerializeField] internal StoryOption[] options = Array.Empty<StoryOption>();

        public string Prompt => prompt;
        public IReadOnlyList<StoryOption> Options => options ?? Array.Empty<StoryOption>();

        public StoryOption Option(string optionId)
        {
            foreach (var option in Options)
            {
                if (string.Equals(option.optionId, optionId, StringComparison.Ordinal))
                {
                    return option;
                }
            }

            throw new KeyNotFoundException($"{id} has no option '{optionId}'.");
        }

        /// <summary>Applies an option to the player's standing. Main thread (campaign state).</summary>
        public void Choose(string optionId, PlayerStanding standing)
        {
            if (standing == null)
            {
                throw new ArgumentNullException(nameof(standing));
            }

            var option = Option(optionId);
            foreach (var change in option.standingChanges ?? Array.Empty<StandingChange>())
            {
                standing.Apply(change);
            }

            foreach (var licence in option.licencesGranted ?? Array.Empty<LicenceDefinition>())
            {
                standing.Grant(licence);
            }

            if (option.scrip != 0)
            {
                standing.AddScrip(option.scrip);
            }

            foreach (var flag in option.flags ?? Array.Empty<string>())
            {
                standing.SetFlag(flag);
            }

            standing.SetFlag($"{id}:{optionId}");
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (Options.Count < 2)
            {
                report.Error(this, nameof(options), "a choice needs at least two options.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var option in Options)
            {
                if (string.IsNullOrWhiteSpace(option.optionId) || !ids.Add(option.optionId))
                {
                    report.Error(this, nameof(options), $"option id '{option.optionId}' is empty or duplicated.");
                }

                foreach (var change in option.standingChanges ?? Array.Empty<StandingChange>())
                {
                    report.NotNull(this, $"{option.optionId}.standingChanges.faction", change.faction);
                }
            }
        }
    }
}
