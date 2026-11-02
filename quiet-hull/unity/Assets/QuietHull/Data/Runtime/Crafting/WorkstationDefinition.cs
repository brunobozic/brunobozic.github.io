using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A workstation the player interacts with: repair station, bench or fabricator.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Crafting/Workstation", fileName = "ws_")]
    public sealed class WorkstationDefinition : DefinitionBase
    {
        [SerializeField] internal WorkstationService services = WorkstationService.Craft;
        [SerializeField] internal RecipeDefinition[] recipes = Array.Empty<RecipeDefinition>();
        [Tooltip("Fabricator tier T1–T5; recipes above it can't be made here.")]
        [Range(1, 5)] [SerializeField] internal int tier = 1;

        [Header("Repair service")]
        [Min(0f)] [SerializeField] internal float repairHealthPerSecond = 40f;
        [Min(0f)] [SerializeField] internal float repairMaxSeconds = 6f;
        [Tooltip("Repairs cost nothing (prologue).")]
        [SerializeField] internal bool repairIsFree = true;

        [Header("Recharge service")]
        [Tooltip("Units charging at once.")]
        [Min(0)] [SerializeField] internal int chargeBays;
        [Tooltip("Multiplies a unit's own recharge rate.")]
        [Min(0f)] [SerializeField] internal float rechargeMultiplier = 1f;

        [Header("Access")]
        [Tooltip("Locked until hacked by the commander (the prologue bay fabricator).")]
        [SerializeField] internal bool requiresHack;
        [Min(0f)] [SerializeField] internal float interactSeconds = 1f;
        [Tooltip("Work needed before it can be used, in order (e.g. cut the welds).")]
        [SerializeField] internal ToolActionKind[] accessRequires = Array.Empty<ToolActionKind>();
        [SerializeField] internal KeyItemDefinition requiresKey;
        [Tooltip("The room is in vacuum: restore atmosphere at its life-support panel before anything works.")]
        [SerializeField] internal bool requiresAtmosphere;
        [Tooltip("Life-support panel id that restores the room's atmosphere.")]
        [SerializeField] internal string atmospherePanelId = string.Empty;
        [Min(0f)] [SerializeField] internal float atmosphereRestoreSeconds = 20f;

        public WorkstationService Services => services;
        public IReadOnlyList<RecipeDefinition> Recipes => recipes ?? Array.Empty<RecipeDefinition>();
        public int Tier => tier;

        public float RepairHealthPerSecond => repairHealthPerSecond;
        public float RepairMaxSeconds => repairMaxSeconds;
        public bool RepairIsFree => repairIsFree;
        public bool RequiresHack => requiresHack;
        public int ChargeBays => chargeBays;
        public float RechargeMultiplier => rechargeMultiplier;
        public IReadOnlyList<ToolActionKind> AccessRequires => accessRequires ?? Array.Empty<ToolActionKind>();
        public KeyItemDefinition RequiresKey => requiresKey;
        public bool RequiresAtmosphere => requiresAtmosphere;
        public string AtmospherePanelId => atmospherePanelId;
        public float AtmosphereRestoreSeconds => atmosphereRestoreSeconds;

        /// <summary>Steps before first use: access work, the key, the atmosphere and the hack.</summary>
        public int AccessStepCount => AccessRequires.Count + (requiresKey != null ? 1 : 0) + (requiresAtmosphere ? 1 : 0) + (requiresHack ? 1 : 0);

        /// <summary>Seconds to fully charge <paramref name="unit"/> here; infinity if it doesn't use charge.</summary>
        public float SecondsToCharge(UnitDefinition unit) =>
            unit != null && unit.UsesCharge && Offers(WorkstationService.Recharge) && rechargeMultiplier > 0f
                ? unit.Charge.SecondsToFull(Array.Empty<StatModifier>()) / rechargeMultiplier
                : float.PositiveInfinity;
        public float InteractSeconds => interactSeconds;

        public bool Offers(WorkstationService service) => (services & service) == service;

        /// <summary>Whether this station can make <paramref name="recipe"/> (tier only; blueprints are checked elsewhere).</summary>
        public bool CanMake(RecipeDefinition recipe) => recipe != null && recipe.MinimumStationTier <= tier;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (services == WorkstationService.None)
            {
                report.Error(this, nameof(services), "a workstation must offer at least one service.");
            }

            if (Offers(WorkstationService.RepairUnit))
            {
                report.Positive(this, nameof(repairHealthPerSecond), repairHealthPerSecond);
                report.Positive(this, nameof(repairMaxSeconds), repairMaxSeconds);
            }

            if (Offers(WorkstationService.Recharge))
            {
                report.Positive(this, nameof(chargeBays), chargeBays);
                report.Positive(this, nameof(rechargeMultiplier), rechargeMultiplier);
            }

            if (requiresAtmosphere && string.IsNullOrWhiteSpace(atmospherePanelId))
            {
                report.Error(this, nameof(atmospherePanelId), "a vacuum room needs a life-support panel id.");
            }

            var crafts = Offers(WorkstationService.Craft) || Offers(WorkstationService.Fabricate);
            if (crafts)
            {
                report.NotEmpty(this, nameof(recipes), recipes);
            }
            else if (Recipes.Count > 0 && !Offers(WorkstationService.Breakdown))
            {
                report.Warning(this, nameof(recipes), "has recipes but offers neither Craft nor Fabricate.");
            }

            foreach (var recipe in Recipes)
            {
                report.NotNull(this, nameof(recipes), recipe);
                if (recipe != null && !CanMake(recipe))
                {
                    report.Error(this, nameof(recipes), $"'{recipe.Id}' needs T{recipe.MinimumStationTier}; this station is T{tier}.");
                }
            }
        }
    }
}
