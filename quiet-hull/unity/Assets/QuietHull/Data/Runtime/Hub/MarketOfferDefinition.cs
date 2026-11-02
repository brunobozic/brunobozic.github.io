using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>Something for sale at a hub: a service, a unit, a module, an augment or an item.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Hub/Market Offer", fileName = "offer_")]
    public sealed class MarketOfferDefinition : DefinitionBase
    {
        [SerializeField] internal OfferKind kind;
        [Tooltip("What you get (null for pure services such as a new identity).")]
        [SerializeField] internal DefinitionBase target;
        [Min(0)] [SerializeField] internal int priceScrip;
        [Tooltip("The story won't continue until it's bought.")]
        [SerializeField] internal bool required;
        [Tooltip("Can be lent for free for the current contract instead of bought.")]
        [SerializeField] internal bool lentForContract;
        [SerializeField] internal string vendor = string.Empty;

        [Header("Teaser")]
        [Tooltip("Shown in the market as a preview but can't be bought yet.")]
        [SerializeField] internal bool teaserOnly;
        [Tooltip("Why it can't be bought yet, e.g. \"Needs Hollow Moon standing: Trusted\".")]
        [SerializeField] internal string unlockRequirement = string.Empty;
        [Tooltip("Every one of these must be met.")]
        [SerializeField] internal Requirement[] requiresAll = System.Array.Empty<Requirement>();
        [Tooltip("At least one of these must be met (empty = no alternative needed).")]
        [SerializeField] internal Requirement[] requiresAnyOf = System.Array.Empty<Requirement>();

        public OfferKind Kind => kind;
        public DefinitionBase Target => target;
        public int PriceScrip => priceScrip;
        public bool Required => required;
        public bool LentForContract => lentForContract;
        public string Vendor => vendor;
        public bool TeaserOnly => teaserOnly;
        public string UnlockRequirement => unlockRequirement;
        public System.Collections.Generic.IReadOnlyList<Requirement> RequiresAll => requiresAll ?? System.Array.Empty<Requirement>();
        public System.Collections.Generic.IReadOnlyList<Requirement> RequiresAnyOf => requiresAnyOf ?? System.Array.Empty<Requirement>();

        /// <summary>For offers with no standing requirements: whether it's for sale at all.</summary>
        public bool CanBuyNow => !teaserOnly;

        /// <summary>Whether the player's standing and licences unlock it (price aside).</summary>
        public bool IsUnlockedFor(PlayerStanding standing)
        {
            if (RequiresAll.Count == 0 && RequiresAnyOf.Count == 0)
            {
                return !teaserOnly;
            }

            foreach (var requirement in RequiresAll)
            {
                if (!requirement.IsMetBy(standing))
                {
                    return false;
                }
            }

            if (RequiresAnyOf.Count == 0)
            {
                return true;
            }

            foreach (var requirement in RequiresAnyOf)
            {
                if (requirement.IsMetBy(standing))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (kind != OfferKind.Service)
            {
                report.NotNull(this, nameof(target), target);
            }

            if (required && lentForContract)
            {
                report.Error(this, nameof(lentForContract), "a required offer can't also be lent.");
            }

            if (teaserOnly && RequiresAll.Count == 0 && RequiresAnyOf.Count == 0)
            {
                report.Error(this, nameof(requiresAll), "a teaser needs requirements that can unlock it.");
            }

            if (teaserOnly && string.IsNullOrWhiteSpace(unlockRequirement))
            {
                report.Error(this, nameof(unlockRequirement), "a teaser must say what unlocks it.");
            }

            if (teaserOnly && required)
            {
                report.Error(this, nameof(teaserOnly), "a required offer can't be a teaser.");
            }

            if (string.IsNullOrWhiteSpace(vendor))
            {
                report.Warning(this, nameof(vendor), "is empty.");
            }
        }
    }
}
