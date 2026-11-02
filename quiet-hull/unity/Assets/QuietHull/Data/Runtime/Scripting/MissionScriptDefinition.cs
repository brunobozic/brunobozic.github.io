using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A whole guided mission: its countdown, rules and ordered beats.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Scripting/Mission Script", fileName = "mission_")]
    public sealed class MissionScriptDefinition : DefinitionBase
    {
        [Min(1f)] [SerializeField] internal float countdownSeconds = 1800f;
        [Tooltip("The countdown stops while a teaching prompt is open.")]
        [SerializeField] internal bool pauseCountdownDuringPrompts = true;
        [Tooltip("False: there's no fail timer; the countdown is the par time used for pacing and par bonuses.")]
        [SerializeField] internal bool countdownIsFailTimer = true;
        [SerializeField] internal MissionBeatDefinition[] beats = Array.Empty<MissionBeatDefinition>();

        public float CountdownSeconds => countdownSeconds;
        public bool PauseCountdownDuringPrompts => pauseCountdownDuringPrompts;
        public bool CountdownIsFailTimer => countdownIsFailTimer;
        public IReadOnlyList<MissionBeatDefinition> Beats => beats ?? Array.Empty<MissionBeatDefinition>();

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.Positive(this, nameof(countdownSeconds), countdownSeconds);
            report.NotEmpty(this, nameof(beats), beats);

            var previousOrder = 0;
            var previousTarget = float.MaxValue;
            var hasCheckpoint = false;
            foreach (var beat in Beats)
            {
                if (beat == null)
                {
                    report.Error(this, nameof(beats), "has an empty slot.");
                    continue;
                }

                if (beat.Order <= previousOrder)
                {
                    report.Error(this, nameof(beats), $"beat '{beat.Id}' (order {beat.Order}) is out of order.");
                }

                if (beat.TargetCountdownSeconds > countdownSeconds)
                {
                    report.Error(this, nameof(beats), $"beat '{beat.Id}' is paced before the countdown starts.");
                }

                // The countdown runs down, so pacing targets must not go back up.
                if (beat.TargetCountdownSeconds > previousTarget)
                {
                    report.Warning(this, nameof(beats), $"beat '{beat.Id}' is paced later on the clock than the beat before it.");
                }

                hasCheckpoint |= beat.IsCheckpoint;
                previousOrder = beat.Order;
                previousTarget = beat.TargetCountdownSeconds;
            }

            if (Beats.Count > 0 && !hasCheckpoint)
            {
                report.Warning(this, nameof(beats), "no checkpoints: a failure restarts the whole mission.");
            }
        }
    }
}
