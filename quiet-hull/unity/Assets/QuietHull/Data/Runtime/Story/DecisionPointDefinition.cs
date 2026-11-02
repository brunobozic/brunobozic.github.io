using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A story decision the company forecast (see the Theme tab). Every option carries the probability
    /// Vashti-Kell's Bio-Research Division assigned to it before the player chose; mission 10 prints the
    /// player's real choices against these numbers. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Story/Decision Point", fileName = "dp_")]
    public sealed class DecisionPointDefinition : DefinitionBase
    {
        /// <summary>The forecast's predicted probabilities must add up to 1 within this tolerance.</summary>
        public const float SumTolerance = 0.011f;

        [Tooltip("How the file heads the entry, e.g. \"Patusan\".")]
        [SerializeField] internal string fileLabel = string.Empty;
        [Tooltip("The mission the decision is made in.")]
        [Min(1)] [SerializeField] internal int mission = 1;
        [SerializeField] internal ForecastOption[] options = Array.Empty<ForecastOption>();
        [Tooltip("Optional: the story choice this decision is (option ids must match).")]
        [SerializeField] internal StoryChoiceDefinition storyChoice;

        public string FileLabel => fileLabel;
        public int Mission => mission;
        public IReadOnlyList<ForecastOption> Options => options ?? Array.Empty<ForecastOption>();
        public StoryChoiceDefinition StoryChoice => storyChoice;

        public bool TryGetOption(string optionId, out ForecastOption option)
        {
            foreach (var candidate in Options)
            {
                if (string.Equals(candidate.optionId, optionId, StringComparison.Ordinal))
                {
                    option = candidate;
                    return true;
                }
            }

            option = default;
            return false;
        }

        public ForecastOption Option(string optionId) =>
            TryGetOption(optionId, out var option) ? option : throw new KeyNotFoundException($"{id} has no option '{optionId}'.");

        /// <summary>The option the forecast rated most likely.</summary>
        public ForecastOption ModalOption
        {
            get
            {
                var best = default(ForecastOption);
                var bestValue = -1f;
                foreach (var option in Options)
                {
                    if (!option.unforecast && option.predicted > bestValue)
                    {
                        best = option;
                        bestValue = option.predicted;
                    }
                }

                return best;
            }
        }

        public ForecastVerdict Classify(string optionId)
        {
            var option = Option(optionId);
            if (option.unforecast)
            {
                return ForecastVerdict.Unforecast;
            }

            return string.Equals(option.optionId, ModalOption.optionId, StringComparison.Ordinal) ? ForecastVerdict.Expected : ForecastVerdict.Deviation;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(fileLabel))
            {
                report.Error(this, nameof(fileLabel), "the file needs a heading for this entry.");
            }

            if (Options.Count < 2)
            {
                report.Error(this, nameof(options), "a decision needs at least two options.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            var sum = 0f;
            var forecastCount = 0;
            foreach (var option in Options)
            {
                if (string.IsNullOrWhiteSpace(option.optionId) || !ids.Add(option.optionId))
                {
                    report.Error(this, nameof(options), $"option id '{option.optionId}' is empty or duplicated.");
                }

                if (string.IsNullOrWhiteSpace(option.forecastLine))
                {
                    report.Error(this, nameof(options), $"{option.optionId}: the file needs a line for it.");
                }

                if (!option.unforecast)
                {
                    forecastCount++;
                    sum += option.predicted;
                    if (option.predicted <= 0f || option.predicted > 1f)
                    {
                        report.Error(this, nameof(options), $"{option.optionId}: a forecast probability is in (0, 1].");
                    }
                }
            }

            if (forecastCount == 0)
            {
                report.Error(this, nameof(options), "the company forecasts at least one option.");
            }
            else if (Math.Abs(sum - 1f) > SumTolerance)
            {
                report.Error(this, nameof(options), $"forecast probabilities add up to {sum:0.###}, not 1.");
            }

            if (storyChoice != null)
            {
                var choiceIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var option in storyChoice.Options)
                {
                    choiceIds.Add(option.optionId);
                }

                if (!choiceIds.SetEquals(ids))
                {
                    report.Error(this, nameof(storyChoice), $"options must match {storyChoice.Id}'s options exactly.");
                }
            }
        }
    }
}
