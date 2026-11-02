using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A multi-part contract from the board (EVE-style chain): each part delivers cargo and pays its share.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Hub/Contract", fileName = "con_")]
    public sealed class ContractDefinition : DefinitionBase
    {
        [SerializeField] internal string client = string.Empty;
        [SerializeField] internal string hulkName = string.Empty;
        [Tooltip("What each part must deliver. Empty for retrieval jobs where you keep what you find.")]
        [SerializeField] internal CargoDefinition deliverable;
        [Tooltip("Pay per part, in order. Its length is the number of parts.")]
        [SerializeField] internal int[] payPerPart = Array.Empty<int>();
        [SerializeField] internal ShuttleModuleDefinition moduleGiven;
        [SerializeField] internal ItemStack[] itemsGiven = Array.Empty<ItemStack>();
        [SerializeField] internal MissionScriptDefinition[] partScripts = Array.Empty<MissionScriptDefinition>();
        [Tooltip("Blueprints given when the whole contract is complete.")]
        [SerializeField] internal BlueprintDefinition[] completionBlueprints = Array.Empty<BlueprintDefinition>();
        [SerializeField] internal StandingChange[] completionStanding = Array.Empty<StandingChange>();
        [Tooltip("One line on the board saying what the job really pays (\"Reward: a conversion kit, +33% damage\").")]
        [SerializeField] internal string rewardSummary = string.Empty;
        [Tooltip("Flags set when the contract completes (they open hub counters).")]
        [SerializeField] internal string[] completionFlags = Array.Empty<string>();

        public string Client => client;
        public string HulkName => hulkName;
        public CargoDefinition Deliverable => deliverable;
        public IReadOnlyList<int> PayPerPart => payPerPart ?? Array.Empty<int>();
        public int Parts => PayPerPart.Count;
        public ShuttleModuleDefinition ModuleGiven => moduleGiven;
        public IReadOnlyList<ItemStack> ItemsGiven => itemsGiven ?? Array.Empty<ItemStack>();
        public IReadOnlyList<MissionScriptDefinition> PartScripts => partScripts ?? Array.Empty<MissionScriptDefinition>();
        public IReadOnlyList<BlueprintDefinition> CompletionBlueprints => completionBlueprints ?? Array.Empty<BlueprintDefinition>();
        public IReadOnlyList<StandingChange> CompletionStanding => completionStanding ?? Array.Empty<StandingChange>();
        public string RewardSummary => rewardSummary;
        public IReadOnlyList<string> CompletionFlags => completionFlags ?? Array.Empty<string>();

        /// <summary>Applies the completion standing and flags to the player.</summary>
        public void Complete(PlayerStanding standing)
        {
            if (standing == null)
            {
                throw new ArgumentNullException(nameof(standing));
            }

            foreach (var change in CompletionStanding)
            {
                standing.Apply(change);
            }

            foreach (var flag in CompletionFlags)
            {
                standing.SetFlag(flag);
            }

            standing.AddScrip(TotalPay);
        }

        public int TotalPay
        {
            get
            {
                var total = 0;
                foreach (var pay in PayPerPart)
                {
                    total = checked(total + pay);
                }

                return total;
            }
        }

        /// <summary>Pay earned after <paramref name="partsCompleted"/> parts.</summary>
        public int PayAfter(int partsCompleted)
        {
            var total = 0;
            var list = PayPerPart;
            for (var i = 0; i < partsCompleted && i < list.Count; i++)
            {
                total = checked(total + list[i]);
            }

            return total;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (deliverable == null && PartScripts.Count == 0)
            {
                report.Error(this, nameof(deliverable), "a contract needs a deliverable or a mission script.");
            }

            report.NotEmpty(this, nameof(payPerPart), payPerPart);
            foreach (var pay in PayPerPart)
            {
                report.NonNegative(this, nameof(payPerPart), pay);
            }

            if (PartScripts.Count != 0 && PartScripts.Count != Parts)
            {
                report.Error(this, nameof(partScripts), $"has {PartScripts.Count} scripts for {Parts} parts.");
            }

            foreach (var blueprint in CompletionBlueprints)
            {
                report.NotNull(this, nameof(completionBlueprints), blueprint);
            }

            if (string.IsNullOrWhiteSpace(client))
            {
                report.Warning(this, nameof(client), "is empty.");
            }
        }
    }
}
