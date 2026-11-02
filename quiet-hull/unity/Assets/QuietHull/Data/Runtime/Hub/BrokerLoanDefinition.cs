using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A bail-out: the broker lends equipment or a military synth for one mission. The price is a
    /// favour, a pro bono job for the broker; one loan at a time until it's repaid. Losing what was
    /// lent costs Scrip on top.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Hub/Broker Loan", fileName = "loan_")]
    public sealed class BrokerLoanDefinition : DefinitionBase
    {
        /// <summary>Set while a favour is owed; the favour contract clears it.</summary>
        public const string FavourOwedFlag = "broker_favour_owed";

        [SerializeField] internal string broker = string.Empty;
        [SerializeField] internal ItemStack[] itemsLent = Array.Empty<ItemStack>();
        [SerializeField] internal UnitDefinition unitLent;
        [SerializeField] internal Mark unitMark = Mark.MkI;
        [Tooltip("The pro bono job that repays it.")]
        [SerializeField] internal ContractDefinition favour;
        [Tooltip("Owed on top if what was lent is destroyed or lost.")]
        [Min(0)] [SerializeField] internal int lossPenaltyScrip;
        [Tooltip("Needs at least this standing with the broker's faction.")]
        [SerializeField] internal FactionDefinition faction;
        [SerializeField] internal Standing minimumStanding = Standing.Neutral;

        public string Broker => broker;
        public IReadOnlyList<ItemStack> ItemsLent => itemsLent ?? Array.Empty<ItemStack>();
        public UnitDefinition UnitLent => unitLent;
        public Mark UnitMark => unitMark;
        public ContractDefinition Favour => favour;
        public int LossPenaltyScrip => lossPenaltyScrip;

        public bool CanBorrow(PlayerStanding standing) =>
            standing != null && !standing.HasFlag(FavourOwedFlag) && (faction == null || standing.Of(faction) >= minimumStanding);

        /// <summary>Takes the loan: marks the favour as owed. Returns false if a favour is already owed.</summary>
        public bool Borrow(PlayerStanding standing)
        {
            if (!CanBorrow(standing))
            {
                return false;
            }

            standing.SetFlag(FavourOwedFlag);
            standing.SetFlag($"{id}:taken");
            return true;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(favour), favour);
            if (ItemsLent.Count == 0 && unitLent == null)
            {
                report.Error(this, nameof(itemsLent), "a loan must lend something.");
            }

            if (favour != null && !favour.IsProBono)
            {
                report.Error(this, nameof(favour), "the favour is pro bono: it must pay 0.");
            }

            if (unitLent != null && unitMark > unitLent.MaxMark)
            {
                report.Error(this, nameof(unitMark), $"{unitLent.Id} doesn't go up to {unitMark}.");
            }

            var clears = false;
            if (favour != null)
            {
                foreach (var flag in favour.ClearsFlags)
                {
                    clears |= flag == FavourOwedFlag;
                }
            }

            if (favour != null && !clears)
            {
                report.Error(this, nameof(favour), "the favour contract must clear the favour-owed flag.");
            }
        }
    }
}
