using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A blueprint that unlocks a recipe when picked up.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Blueprint", fileName = "bp_")]
    public sealed class BlueprintDefinition : ItemDefinition
    {
        [Header("Blueprint")]
        [SerializeField] internal RecipeDefinition unlocks;

        public override ItemCategory Category => ItemCategory.Blueprint;
        public RecipeDefinition Unlocks => unlocks;

        protected override void ValidateItem(ValidationReport report) => report.NotNull(this, nameof(unlocks), unlocks);
    }
}
