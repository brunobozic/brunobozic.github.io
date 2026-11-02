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

        public OfferKind Kind => kind;
        public DefinitionBase Target => target;
        public int PriceScrip => priceScrip;
        public bool Required => required;
        public bool LentForContract => lentForContract;
        public string Vendor => vendor;
        public bool TeaserOnly => teaserOnly;
        public string UnlockRequirement => unlockRequirement;
        public bool CanBuyNow => !teaserOnly;

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
