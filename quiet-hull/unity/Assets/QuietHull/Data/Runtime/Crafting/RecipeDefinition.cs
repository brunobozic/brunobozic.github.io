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
        [Tooltip("Non-material inputs (a grav-lattice alloy unit, a myomer bundle).")]
        [SerializeField] internal ItemStack[] itemInputs = Array.Empty<ItemStack>();
        [Tooltip("Lowest fabricator tier (T1–T5) that can make it.")]
        [Range(1, 5)] [SerializeField] internal int minimumStationTier = 1;
        [Min(0f)] [SerializeField] internal float craftSeconds = 5f;

        [Tooltip("If set, the recipe is locked until this blueprint is learned or the station is hacked.")]
        [SerializeField] internal bool requiresBlueprint;

        public ItemDefinition Output => output;
        public int OutputCount => outputCount;
        public IReadOnlyList<MaterialCost> Inputs => inputs ?? Array.Empty<MaterialCost>();
        public IReadOnlyList<ItemStack> ItemInputs => itemInputs ?? Array.Empty<ItemStack>();
        public int MinimumStationTier => minimumStationTier;
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
            if (Inputs.Count == 0 && ItemInputs.Count == 0)
            {
                report.Error(this, nameof(inputs), "a recipe needs at least one input.");
            }

            foreach (var stack in ItemInputs)
            {
                report.NotNull(this, $"{nameof(itemInputs)}.item", stack.item);
                report.Positive(this, $"{nameof(itemInputs)}.count", stack.count);
            }

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
