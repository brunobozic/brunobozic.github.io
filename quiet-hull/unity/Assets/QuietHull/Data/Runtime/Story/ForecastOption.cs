using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One option of a decision point, with the probability the company assigned to it beforehand.</summary>
    [Serializable]
    public struct ForecastOption
    {
        public string optionId;
        [Tooltip("How the player sees the option.")]
        public string label;
        [Tooltip("How the company's file words it, e.g. \"release via ventilation (Open)\".")]
        public string forecastLine;
        [Tooltip("The company's predicted probability, 0..1. Ignored when unforecast.")]
        [Range(0f, 1f)] public float predicted;
        [Tooltip("The model has no number for this option: the file prints \"—\".")]
        public bool unforecast;

        public ForecastOption(string optionId, string label, string forecastLine, float predicted)
        {
            this.optionId = optionId;
            this.label = label;
            this.forecastLine = forecastLine;
            this.predicted = predicted;
            unforecast = false;
        }

        public static ForecastOption Unforecast(string optionId, string label, string forecastLine) =>
            new ForecastOption(optionId, label, forecastLine, 0f) { unforecast = true };
    }
}
