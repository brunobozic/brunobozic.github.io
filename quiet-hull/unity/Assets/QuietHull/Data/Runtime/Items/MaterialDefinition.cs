using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A stackable crafting material (Plating, Circuitry, Ordnance, scrap).</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Material", fileName = "mat_")]
    public sealed class MaterialDefinition : ItemDefinition
    {
        [Header("Material")]
        [Min(1)] [SerializeField] internal int maxStack = 999;

        public override ItemCategory Category => ItemCategory.Material;
        public int MaxStack => maxStack;

        protected override void ValidateItem(ValidationReport report) => report.Positive(this, nameof(maxStack), maxStack);
    }
}
