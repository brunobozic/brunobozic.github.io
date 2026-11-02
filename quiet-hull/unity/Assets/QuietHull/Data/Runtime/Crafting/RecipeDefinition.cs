using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>Turns materials into an item at a workstation.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Crafting/Recipe", fileName = "rcp_")]
    public sealed class RecipeDefinition : DefinitionBase
    {
        [SerializeField] internal ItemDefinition output;
        [Min(1)] [SerializeField] internal int outputCount = 1;
        [SerializeField] internal MaterialCost[] inputs = Array.Empty<MaterialCost>();
        [Min(0f)] [SerializeField] internal float craftSeconds = 5f;

        [Tooltip("If set, the recipe is locked until this blueprint is learned or the station is hacked.")]
        [SerializeField] internal bool requiresBlueprint;

        public ItemDefinition Output => output;
        public int OutputCount => outputCount;
        public IReadOnlyList<MaterialCost> Inputs => inputs ?? Array.Empty<MaterialCost>();
        public float CraftSeconds => craftSeconds;
        public bool RequiresBlueprint => requiresBlueprint;

        /// <summary>Amount of <paramref name="material"/> one craft consumes.</summary>
        public int AmountOf(MaterialDefinition material)
        {
            var total = 0;
            foreach (var cost in Inputs)
            {
                if (cost.material == material)
                {
                    total += cost.amount;
                }
            }

            return total;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(output), output);
            report.Positive(this, nameof(outputCount), outputCount);
            report.NotEmpty(this, nameof(inputs), inputs);

            var seen = new HashSet<MaterialDefinition>();
            foreach (var cost in Inputs)
            {
                report.NotNull(this, $"{nameof(inputs)}.material", cost.material);
                report.Positive(this, $"{nameof(inputs)}.amount", cost.amount);
                if (cost.material != null && !seen.Add(cost.material))
                {
                    report.Warning(this, nameof(inputs), $"material '{cost.material.Id}' is listed twice; merge the entries.");
                }
            }
        }
    }
}
