using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// The run's card targeting: the cards slotted (each has a built-in Echo), plus the card-targeting
    /// Catalysts: a Beacon on one card, a Sigil on one family or suit, an Echo Catalyst on the slotted cards.
    /// Immutable; build one per run with the With* methods.
    /// </summary>
    public sealed class CardTargeting
    {
        public static readonly CardTargeting None = new CardTargeting(null, Rarity.Common, false, OmenFamily.Hive, OmenSuit.None, Rarity.Common,
            Array.Empty<OmenCardDefinition>(), Rarity.Common, false);

        private CardTargeting(OmenCardDefinition beaconCard, Rarity beaconRarity, bool hasSigil, OmenFamily sigilFamily, OmenSuit sigilSuit,
            Rarity sigilRarity, IReadOnlyList<OmenCardDefinition> echoCards, Rarity echoRarity, bool hasEcho)
        {
            BeaconCard = beaconCard;
            BeaconRarity = beaconRarity;
            HasSigil = hasSigil;
            SigilFamily = sigilFamily;
            SigilSuit = sigilSuit;
            SigilRarity = sigilRarity;
            EchoCards = echoCards;
            EchoRarity = echoRarity;
            HasEcho = hasEcho;
        }

        public OmenCardDefinition BeaconCard { get; }
        public Rarity BeaconRarity { get; }
        public bool HasBeacon => BeaconCard != null;

        public bool HasSigil { get; }
        public OmenFamily SigilFamily { get; }

        /// <summary>When set, the Sigil targets this suit of the Minor Arcana instead of a family.</summary>
        public OmenSuit SigilSuit { get; }
        public Rarity SigilRarity { get; }

        /// <summary>An Echo Catalyst is slotted (on top of the built-in Echo every slotted card has).</summary>
        public bool HasEcho { get; }

        /// <summary>The cards slotted this run; each claims the built-in Echo share.</summary>
        public IReadOnlyList<OmenCardDefinition> EchoCards { get; }
        public Rarity EchoRarity { get; }

        public CardTargeting WithBeacon(OmenCardDefinition card, Rarity rarity) =>
            new CardTargeting(card ?? throw new ArgumentNullException(nameof(card)), rarity, HasSigil, SigilFamily, SigilSuit, SigilRarity, EchoCards, EchoRarity, HasEcho);

        public CardTargeting WithSigil(OmenFamily family, Rarity rarity) =>
            new CardTargeting(BeaconCard, BeaconRarity, true, family, OmenSuit.None, rarity, EchoCards, EchoRarity, HasEcho);

        public CardTargeting WithSigil(OmenSuit suit, Rarity rarity) =>
            new CardTargeting(BeaconCard, BeaconRarity, true, OmenFamily.Suit, suit, rarity, EchoCards, EchoRarity, HasEcho);

        /// <summary>The run's slotted cards, for the built-in Echo (copied; later changes to the list don't leak in).</summary>
        public CardTargeting WithSlotted(IEnumerable<OmenCardDefinition> slotted) =>
            new CardTargeting(BeaconCard, BeaconRarity, HasSigil, SigilFamily, SigilSuit, SigilRarity,
                new List<OmenCardDefinition>(slotted ?? throw new ArgumentNullException(nameof(slotted))).AsReadOnly(), EchoRarity, HasEcho);

        /// <summary>An Echo Catalyst on the run's slotted cards (copy the list; later changes to it don't leak in).</summary>
        public CardTargeting WithEcho(IEnumerable<OmenCardDefinition> slotted, Rarity rarity) =>
            new CardTargeting(BeaconCard, BeaconRarity, HasSigil, SigilFamily, SigilSuit, SigilRarity,
                new List<OmenCardDefinition>(slotted ?? throw new ArgumentNullException(nameof(slotted))).AsReadOnly(), rarity, true);

        public bool SigilMatches(OmenCardDefinition card) =>
            HasSigil && card != null && (SigilSuit != OmenSuit.None ? card.Suit == SigilSuit : card.Family == SigilFamily);
    }
}
