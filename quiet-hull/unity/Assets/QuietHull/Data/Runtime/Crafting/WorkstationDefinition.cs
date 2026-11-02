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

        [Header("Repair service")]
        [Min(0f)] [SerializeField] internal float repairHealthPerSecond = 40f;
        [Min(0f)] [SerializeField] internal float repairMaxSeconds = 6f;
        [Tooltip("Repairs cost nothing (prologue).")]
        [SerializeField] internal bool repairIsFree = true;

        [Header("Access")]
        [Tooltip("Locked until hacked by the commander (the prologue bay fabricator).")]
        [SerializeField] internal bool requiresHack;
        [Min(0f)] [SerializeField] internal float interactSeconds = 1f;

        public WorkstationService Services => services;
        public IReadOnlyList<RecipeDefinition> Recipes => recipes ?? Array.Empty<RecipeDefinition>();
        public float RepairHealthPerSecond => repairHealthPerSecond;
        public float RepairMaxSeconds => repairMaxSeconds;
        public bool RepairIsFree => repairIsFree;
        public bool RequiresHack => requiresHack;
        public float InteractSeconds => interactSeconds;

        public bool Offers(WorkstationService service) => (services & service) == service;

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

            var crafts = Offers(WorkstationService.Craft) || Offers(WorkstationService.Fabricate);
            if (crafts)
            {
                report.NotEmpty(this, nameof(recipes), recipes);
            }
            else if (Recipes.Count > 0)
            {
                report.Warning(this, nameof(recipes), "has recipes but offers neither Craft nor Fabricate.");
            }

            foreach (var recipe in Recipes)
            {
                report.NotNull(this, nameof(recipes), recipe);
            }
        }
    }
}
