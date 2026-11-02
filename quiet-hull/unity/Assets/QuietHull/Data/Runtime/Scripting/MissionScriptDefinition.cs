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

        [Header("Progression")]
        [Tooltip("Commander XP for completing the mission.")]
        [Min(0)] [SerializeField] internal int commanderXp;
        [Tooltip("Countermeasure cards this mission adds to Marlow's hand.")]
        [SerializeField] internal CountermeasureCardDefinition[] cardsGranted = Array.Empty<CountermeasureCardDefinition>();
        [Tooltip("The long-term goal shown on the goals panel when this mission ends.")]
        [SerializeField] internal GoalDefinition nextGoal;
        [SerializeField] internal LoreLogDefinition[] loreLogs = Array.Empty<LoreLogDefinition>();
        [Tooltip("Flags set when the mission completes (they open hub counters and goal steps).")]
        [SerializeField] internal string[] completionFlags = Array.Empty<string>();
        [Tooltip("Workers drain charge (off in the prologue, where the lesson load is already high).")]
        [SerializeField] internal bool chargeEnabled = true;

        [Header("Safety nets")]
        [Tooltip("After this many failures the reader offers the mercy card free (0 = never).")]
        [Min(0)] [SerializeField] internal int mercyAfterFailures;
        [SerializeField] internal OmenCardDefinition mercyOmen;
        [Tooltip("Events that autosave (e.g. every turn of the hulk).")]
        [SerializeField] internal string[] autosaveOnEvents = Array.Empty<string>();

        public float CountdownSeconds => countdownSeconds;
        public bool PauseCountdownDuringPrompts => pauseCountdownDuringPrompts;
        public bool CountdownIsFailTimer => countdownIsFailTimer;
        public IReadOnlyList<MissionBeatDefinition> Beats => beats ?? Array.Empty<MissionBeatDefinition>();
        public int CommanderXp => commanderXp;
        public IReadOnlyList<CountermeasureCardDefinition> CardsGranted => cardsGranted ?? Array.Empty<CountermeasureCardDefinition>();
        public GoalDefinition NextGoal => nextGoal;
        public IReadOnlyList<LoreLogDefinition> LoreLogs => loreLogs ?? Array.Empty<LoreLogDefinition>();
        public IReadOnlyList<string> CompletionFlags => completionFlags ?? Array.Empty<string>();
        public bool ChargeEnabled => chargeEnabled;

        /// <summary>Applies completion flags and commander XP bookkeeping to the player.</summary>
        public void Complete(PlayerStanding standing)
        {
            if (standing == null)
            {
                throw new ArgumentNullException(nameof(standing));
            }

            foreach (var flag in CompletionFlags)
            {
                standing.SetFlag(flag);
            }

            standing.AddCommanderXp(commanderXp);
        }
        public int MercyAfterFailures => mercyAfterFailures;
        public OmenCardDefinition MercyOmen => mercyOmen;
        public IReadOnlyList<string> AutosaveOnEvents => autosaveOnEvents ?? Array.Empty<string>();

        public int CountPrompts(PromptStyle style)
        {
            var count = 0;
            foreach (var beat in Beats)
            {
                if (beat != null && beat.PromptStyle == style && !string.IsNullOrWhiteSpace(beat.PromptTitle))
                {
                    count++;
                }
            }

            return count;
        }

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

            if (mercyAfterFailures > 0)
            {
                report.NotNull(this, nameof(mercyOmen), mercyOmen);
            }

            if (Beats.Count > 0 && !hasCheckpoint)
            {
                report.Warning(this, nameof(beats), "no checkpoints: a failure restarts the whole mission.");
            }
        }
    }
}
