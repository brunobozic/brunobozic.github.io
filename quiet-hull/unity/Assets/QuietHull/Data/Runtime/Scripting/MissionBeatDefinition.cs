using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// One beat of a guided mission (the prologue has 23). A beat starts when its trigger fires,
    /// opens objectives, shows a teaching prompt and may start waves. Beats are data so designers
    /// can retime the mission without code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Scripting/Mission Beat", fileName = "beat_")]
    public sealed class MissionBeatDefinition : DefinitionBase
    {
        [Min(1)] [SerializeField] internal int order = 1;

        [Tooltip("Countdown value (seconds) the beat is paced for; used by the pacing report, not as a trigger.")]
        [Min(0f)] [SerializeField] internal float targetCountdownSeconds;

        [SerializeField] internal BeatTrigger trigger = BeatTrigger.PreviousBeatCompleted;
        [Tooltip("Room id, item id, recipe id, wave id or timer seconds, depending on the trigger.")]
        [SerializeField] internal string triggerArgument = string.Empty;

        [SerializeField] internal ObjectiveEntry[] objectives = Array.Empty<ObjectiveEntry>();

        [Header("Teaching prompt")]
        [SerializeField] internal string promptTitle = string.Empty;
        [Tooltip("Card for ideas a player can't guess; Toast for a one-liner; None when the level teaches it.")]
        [SerializeField] internal PromptStyle promptStyle = PromptStyle.Card;
        [TextArea(2, 6)] [SerializeField] internal string promptBody = string.Empty;
        [Tooltip("What the beat teaches; shown on the results screen checklist.")]
        [SerializeField] internal string teaches = string.Empty;

        [Header("Flow")]
        [SerializeField] internal bool isCheckpoint;
        [SerializeField] internal WaveDefinition[] startsWaves = Array.Empty<WaveDefinition>();

        public int Order => order;
        public float TargetCountdownSeconds => targetCountdownSeconds;
        public BeatTrigger Trigger => trigger;
        public string TriggerArgument => triggerArgument;
        public IReadOnlyList<ObjectiveEntry> Objectives => objectives ?? Array.Empty<ObjectiveEntry>();
        public string PromptTitle => promptTitle;
        public string PromptBody => promptBody;
        public string Teaches => teaches;
        public PromptStyle PromptStyle => promptStyle;
        public bool HasPrompt => !string.IsNullOrWhiteSpace(promptTitle) && promptStyle != PromptStyle.None;
        public bool IsCheckpoint => isCheckpoint;
        public IReadOnlyList<WaveDefinition> StartsWaves => startsWaves ?? Array.Empty<WaveDefinition>();

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.Positive(this, nameof(order), order);

            var needsArgument = trigger != BeatTrigger.MissionStart && trigger != BeatTrigger.PreviousBeatCompleted;
            if (needsArgument && string.IsNullOrWhiteSpace(triggerArgument))
            {
                report.Error(this, nameof(triggerArgument), $"trigger {trigger} needs an argument.");
            }

            if (trigger == BeatTrigger.TimerReached && !float.TryParse(triggerArgument, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                report.Error(this, nameof(triggerArgument), "TimerReached needs a number of seconds.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var objective in Objectives)
            {
                if (string.IsNullOrWhiteSpace(objective.objectiveId) || !ids.Add(objective.objectiveId))
                {
                    report.Error(this, nameof(objectives), $"objective id '{objective.objectiveId}' is empty or duplicated.");
                }
            }

            foreach (var wave in StartsWaves)
            {
                report.NotNull(this, nameof(startsWaves), wave);
            }
        }
    }
}
