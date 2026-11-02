using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One checklist line of a long-term goal.</summary>
    [Serializable]
    public struct GoalStep
    {
        public string label;
        public GoalStepKind kind;
        public ItemDefinition item;
        [Min(1)] public int count;
        public string flag;
        public FactionDefinition faction;
        public Standing standing;
        [Tooltip("Where to get it, shown when the step isn't done (\"Kill zones; the Tremolino parts store\").")]
        public string hint;

        public bool IsDone(CampaignState state)
        {
            switch (kind)
            {
                case GoalStepKind.HaveMaterial:
                case GoalStepKind.HaveItem:
                    return state.Count(item) >= count;
                case GoalStepKind.HaveFlag:
                    return state.Standing.HasFlag(flag);
                case GoalStepKind.StandingAtLeast:
                    return state.Standing.Of(faction) >= standing;
                case GoalStepKind.HaveScrip:
                    return state.Standing.Scrip >= count;
                default:
                    return false;
            }
        }

        /// <summary>Progress toward the step, 0–1 (partial for counts).</summary>
        public float Progress(CampaignState state)
        {
            if (kind == GoalStepKind.HaveMaterial || kind == GoalStepKind.HaveItem)
            {
                return count <= 0 ? 1f : Math.Min(1f, state.Count(item) / (float)count);
            }

            if (kind == GoalStepKind.HaveScrip)
            {
                return count <= 0 ? 1f : Math.Min(1f, state.Standing.Scrip / (float)count);
            }

            return IsDone(state) ? 1f : 0f;
        }
    }

    /// <summary>
    /// A long-term goal: the third line of the goals panel ("Working toward: Servo arms Mk II, 3 of 5").
    /// It stays on screen across missions, so the player always sees the next thing.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Campaign/Goal", fileName = "goal_")]
    public sealed class GoalDefinition : DefinitionBase
    {
        [SerializeField] internal GoalStep[] steps = Array.Empty<GoalStep>();
        [Tooltip("What finishing it pays, shown beside the goal.")]
        [SerializeField] internal string payoff = string.Empty;

        public IReadOnlyList<GoalStep> Steps => steps ?? Array.Empty<GoalStep>();
        public string Payoff => payoff;

        public int StepsDone(CampaignState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var done = 0;
            foreach (var step in Steps)
            {
                if (step.IsDone(state))
                {
                    done++;
                }
            }

            return done;
        }

        public bool IsComplete(CampaignState state) => StepsDone(state) == Steps.Count;

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotEmpty(this, nameof(steps), steps);
            foreach (var step in Steps)
            {
                if (string.IsNullOrWhiteSpace(step.label) || string.IsNullOrWhiteSpace(step.hint))
                {
                    report.Error(this, nameof(steps), "every step needs a label and a hint.");
                }

                var needsItem = step.kind == GoalStepKind.HaveMaterial || step.kind == GoalStepKind.HaveItem;
                if (needsItem && step.item == null)
                {
                    report.Error(this, nameof(steps), $"'{step.label}' needs an item.");
                }

                if (step.kind == GoalStepKind.HaveFlag && string.IsNullOrWhiteSpace(step.flag))
                {
                    report.Error(this, nameof(steps), $"'{step.label}' needs a flag.");
                }

                if (step.kind == GoalStepKind.StandingAtLeast && step.faction == null)
                {
                    report.Error(this, nameof(steps), $"'{step.label}' needs a faction.");
                }
            }
        }
    }
}
