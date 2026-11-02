using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A work tool: the arc welder and the molecular disassembler in the prologue.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Tool", fileName = "itm_tool_")]
    public sealed class ToolDefinition : ItemDefinition
    {
        [Header("Tool")]
        [Min(0.1f)] [SerializeField] internal float range = 1.5f;
        [SerializeField] internal ToolAction[] actions = Array.Empty<ToolAction>();

        public override ItemCategory Category => ItemCategory.Tool;
        public float Range => range;
        public IReadOnlyList<ToolAction> Actions => actions ?? Array.Empty<ToolAction>();

        public bool TryGetAction(ToolActionKind kind, out ToolAction action)
        {
            foreach (var candidate in Actions)
            {
                if (candidate.kind == kind)
                {
                    action = candidate;
                    return true;
                }
            }

            action = default;
            return false;
        }

        protected override void ValidateItem(ValidationReport report)
        {
            report.Positive(this, nameof(range), range);
            report.NotEmpty(this, nameof(actions), actions);

            var seen = new HashSet<ToolActionKind>();
            foreach (var action in Actions)
            {
                if (!seen.Add(action.kind))
                {
                    report.Error(this, nameof(actions), $"action {action.kind} is listed twice.");
                }

                report.Positive(this, $"{nameof(actions)}.{action.kind}.durationSeconds", action.durationSeconds);
            }
        }
    }
}
