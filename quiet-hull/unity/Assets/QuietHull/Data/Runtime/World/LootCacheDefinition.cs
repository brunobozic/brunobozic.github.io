using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A hand-placed cache of materials and items: a shelf, a corpse, a locked cage.
    /// Guided missions place these so the materials add up exactly to what the player must craft.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Loot Cache", fileName = "cache_")]
    public sealed class LootCacheDefinition : DefinitionBase
    {
        [SerializeField] internal MaterialCost[] materials = Array.Empty<MaterialCost>();
        [SerializeField] internal ItemStack[] items = Array.Empty<ItemStack>();
        [Tooltip("Tools needed to get in, e.g. the welder for bolts and the disassembler for debris.")]
        [SerializeField] internal ToolActionKind[] accessRequires = Array.Empty<ToolActionKind>();
        [Tooltip("How many copies the level places (e.g. 10 bulkhead panels).")]
        [Min(1)] [SerializeField] internal int placedCount = 1;

        public IReadOnlyList<MaterialCost> Materials => materials ?? Array.Empty<MaterialCost>();
        public IReadOnlyList<ItemStack> Items => items ?? Array.Empty<ItemStack>();
        public IReadOnlyList<ToolActionKind> AccessRequires => accessRequires ?? Array.Empty<ToolActionKind>();
        public int PlacedCount => placedCount;

        /// <summary>Total of <paramref name="material"/> across every placed copy.</summary>
        public int TotalAmountOf(MaterialDefinition material) => checked(AmountOf(material) * placedCount);

        public int AmountOf(MaterialDefinition material)
        {
            var total = 0;
            foreach (var cost in Materials)
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
            report.Positive(this, nameof(placedCount), placedCount);
            if (Materials.Count == 0 && Items.Count == 0)
            {
                report.Warning(this, nameof(materials), "the cache is empty.");
            }

            foreach (var cost in Materials)
            {
                report.NotNull(this, $"{nameof(materials)}.material", cost.material);
                report.Positive(this, $"{nameof(materials)}.amount", cost.amount);
            }

            foreach (var stack in Items)
            {
                report.NotNull(this, $"{nameof(items)}.item", stack.item);
                report.Positive(this, $"{nameof(items)}.count", stack.count);
            }
        }
    }
}
