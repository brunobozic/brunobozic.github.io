using System;
using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// The tarot rollout for missions 1–10: the schedule (Major and Minor slots, drops, rarity ceilings, the
    /// Cartomancer's table, the bench, mutations, corruption) and every card's arrival. Cards for missions 1–3
    /// come from the earlier packs; this pack adds 16 Major and 5 Minor cards, the Anchorite (Act 3), the
    /// Cartomancer's rules and the 19 Catalysts.
    /// Values mirror the "Tarot rollout: missions 1–10" tab of the design doc.
    /// </summary>
    public static class OmenRolloutBuilder
    {
        public const string CatalogId = "cat_omen_rollout";
        public const string RolloutId = "rollout_omens_m1_m10";

        /// <summary>Campaign flags of the ledger choice (mission 3) that pick the mission 6 reward.</summary>
        public const string LedgerReturnedFlag = "ledger_returned";
        public const string LedgerSoldFlag = "ledger_sold";
        public const string LedgerKeptFlag = "ledger_kept";

        /// <summary>
        /// Planning figure: commander XP of each of missions 4 and 5. At 150 (mission 3's figure) a player who skipped
        /// the Judea is 49 XP short of Rank 7 at mission 6; 180 lands it at 880 either way.
        /// </summary>
        public const int PlannedXpPerMission = 180;

        private const string Folder = "Omens";

        public static OmenRolloutContent Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var previous = TremolinoContentBuilder.Build(factory);
            var make = new DefinitionMaker(factory);
            var c = new OmenRolloutContent { Previous = previous };

            BuildCards(c, make);
            BuildMinorArcana(c, make);
            BuildAnchorite(c, make);
            BuildCartomancer(c, make);
            c.Catalysts = CatalystCatalogBuilder.Build(make);
            BuildRollout(c, make);

            c.Catalog = make.WriteCatalog(CatalogId);
            c.All = make.All;
            return c;
        }

        private static OmenCardDefinition Card(DefinitionMaker make, string id, string name, string description, OmenFamily family,
            Rarity rarity, int commonThreat, string legendaryTwist, params OmenEffect[] effects)
        {
            var card = make.Def<OmenCardDefinition>(id, Folder, name, description, "omen", family.ToString().ToLowerInvariant(), "rollout");
            card.family = family;
            card.rarity = rarity;
            card.commonThreat = commonThreat;
            card.effects = effects;
            card.legendaryTwist = legendaryTwist;
            return card;
        }

        private static OmenEffect Benefit(OmenEffectKind kind, ScalingRule rule, float value) => new OmenEffect(kind, OmenSide.Benefit, rule, value);

        private static OmenEffect Handicap(OmenEffectKind kind, ScalingRule rule, float value) => new OmenEffect(kind, OmenSide.Handicap, rule, value);

        private static void BuildCards(OmenRolloutContent c, DefinitionMaker make)
        {
            var cs = c.Previous.Previous;

            // ---- Mission 4: Pod 7 ---------------------------------------------------------------
            var ironVow = Card(make, "omen_iron_vow", "Iron Vow",
                "Vow: no unit Wrecked. Keep it for more loot and pay; break it and the hive knows.", OmenFamily.Vow, Rarity.Common, 0,
                "While the vow holds, Disabled units self-repair to 25%.",
                Benefit(OmenEffectKind.LootPercent, ScalingRule.Linear, 30f),
                Benefit(OmenEffectKind.ContractPayPercent, ScalingRule.Linear, 20f));
            ironVow.vowCondition = VowCondition.NoUnitWrecked;
            ironVow.brokenEffects = new[] { Handicap(OmenEffectKind.AwarenessJumpPercent, ScalingRule.Linear, 20f) };
            c.IronVow = ironVow;

            c.HungryHive = Card(make, "omen_hungry_hive", "Hungry Hive",
                "Surges 25% larger; +1 rarity roll on everything.", OmenFamily.Hive, Rarity.Common, 3,
                "Champions in a surge drop a Catalyst.",
                Handicap(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 25f),
                Benefit(OmenEffectKind.RarityRolls, ScalingRule.Count, 1f));

            // ---- Mission 5: the Nan-Shan --------------------------------------------------------
            var leaper = Card(make, "omen_culling_leaper", "Culling: Leaper",
                "Far fewer Leapers; the hulk gives a little less.", OmenFamily.Culling, Rarity.Common, -2,
                "No Leapers at all; the rest of the swarm is 10% larger.",
                Benefit(OmenEffectKind.CulledSpawnReduction, ScalingRule.Reduction, 0.6f),
                Handicap(OmenEffectKind.LootPercent, ScalingRule.Linear, -15f));
            leaper.culledAlien = cs.Leaper;
            c.CullingLeaper = leaper;

            c.IonStorm = Card(make, "omen_ion_storm", "Ion Storm",
                "Arc intensity +1 on the hulk; arc affixes and Circuitry in the loot.", OmenFamily.Condition, Rarity.Common, 2,
                "Every arc item gets +1 socket.",
                Handicap(OmenEffectKind.ConditionIntensity, ScalingRule.Linear, 1f),
                Benefit(OmenEffectKind.AffixBias, ScalingRule.Chance, 0.30f),
                Benefit(OmenEffectKind.CurrencyPercent, ScalingRule.Linear, 50f));

            // ---- Mission 6: the ledger's price ---------------------------------------------------
            c.BrittleWelds = Card(make, "omen_brittle_welds", "Brittle Welds",
                "Welds hold half as long; sentry guns in the loot.", OmenFamily.Ship, Rarity.Common, 2,
                "Sentries you place start with a full magazine.",
                Handicap(OmenEffectKind.WeldDurationPercent, ScalingRule.Linear, -50f),
                Benefit(OmenEffectKind.AffixBias, ScalingRule.Chance, 0.30f));

            // Fortune folded into Candles (review 2): The Deck and Prism Eclipse are Minor cards.
            c.TheDeck = Minor(make, "omen_the_deck", "The Deck",
                "Candles: one more Omen card from Requisition; the aliens hit a little harder.", OmenSuit.Candles,
                "Every Omen dropped this run is one rarity higher.",
                Benefit(OmenEffectKind.OmenDropCount, ScalingRule.Count, 1f),
                Handicap(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 10f));

            var chitin = Card(make, "omen_iron_chitin", "Iron Chitin",
                "Tougher, harder-hitting aliens; choose the reward: more loot or more pay.", OmenFamily.Brutality, Rarity.Common, 3,
                "Chitin drops ×2 from champions.",
                Handicap(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 20f),
                Handicap(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 10f));
            chitin.brutalityPayout = true;
            c.IronChitin = chitin;

            // ---- Mission 7: old patterns ---------------------------------------------------------
            c.EngineeringDeck = Card(make, "omen_engineering_deck", "Engineering Deck",
                "The hulk has a T3 fabricator or better; bigger surges guard it.", OmenFamily.Fabrication, Rarity.Common, 2,
                "The fabricator comes with one random Mk VI recipe.",
                Benefit(OmenEffectKind.FabricatorTierMinimum, ScalingRule.None, 3f),
                Handicap(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 15f));

            c.TheFortress = Card(make, "omen_the_fortress", "The Fortress",
                "Hybrid: fewer aliens, but each much tougher.", OmenFamily.Hybrid, Rarity.Common, 1,
                "Champions can't spawn.",
                Benefit(OmenEffectKind.SurgeSizePercent, ScalingRule.Reduction, 0.30f),
                Handicap(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 40f));

            // ---- Mission 8: the Sephora ----------------------------------------------------------
            c.TheWatcher = Card(make, "omen_the_watcher", "The Watcher",
                "A Stalker that hunts only noise from the bay; signature-suppression affixes in the loot.", OmenFamily.Hive, Rarity.Rare, 3,
                "The Watcher drops a Legendary augment blueprint fragment.",
                Handicap(OmenEffectKind.AwarenessJumpPercent, ScalingRule.Linear, 25f),
                Benefit(OmenEffectKind.AffixBias, ScalingRule.Chance, 0.35f));

            c.LongNight = Card(make, "omen_long_night", "Long Night",
                "Stage 1 ends 3 minutes sooner; +1 rarity roll on the Reliquary.", OmenFamily.Ship, Rarity.Common, 3,
                "The Reliquary always holds an Epic.",
                Handicap(OmenEffectKind.HiveClockSeconds, ScalingRule.Linear, -180f),
                Benefit(OmenEffectKind.RarityRolls, ScalingRule.Count, 1f));

            // ---- Mission 9: second breach --------------------------------------------------------
            c.SecondBreach = Card(make, "omen_second_breach", "Second Breach",
                "A second hive spawn mid-ship; +1 epic roll and augment loot.", OmenFamily.Hive, Rarity.Common, 4,
                "Both hives drop a guaranteed Rare augment.",
                Handicap(OmenEffectKind.ExtraHiveSpawns, ScalingRule.Count, 1f),
                Benefit(OmenEffectKind.RarityRolls, ScalingRule.Count, 1f));

            var spitter = Card(make, "omen_culling_spitter", "Culling: Spitter",
                "Far fewer Spitters; the hulk gives a little less.", OmenFamily.Culling, Rarity.Common, -2,
                "No Spitters at all; Crawlers are 10% faster.",
                Benefit(OmenEffectKind.CulledSpawnReduction, ScalingRule.Reduction, 0.6f),
                Handicap(OmenEffectKind.LootPercent, ScalingRule.Linear, -15f));
            spitter.culledAlien = c.Previous.Previous.Prologue.Spitter;
            c.CullingSpitter = spitter;

            c.ChampionTide = Card(make, "omen_champion_tide", "Champion Tide",
                "Champions twice as common; Catalyst drops ×2.", OmenFamily.Hive, Rarity.Common, 4,
                "Every champion drops a Catalyst.",
                Handicap(OmenEffectKind.ChampionMultiplier, ScalingRule.Multiplier, 2f),
                Benefit(OmenEffectKind.CatalystMultiplier, ScalingRule.Multiplier, 2f));

            // ---- Mission 10: the Ledger ----------------------------------------------------------
            var blood = Card(make, "omen_blood_price", "Blood Price",
                "Aliens +50% health and damage, for a large payout.", OmenFamily.Brutality, Rarity.Common, 6,
                "Kills restore 1% hull to the unit that made them.",
                Handicap(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 50f),
                Handicap(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 50f));
            blood.brutalityPayout = true;
            c.BloodPrice = blood;

            c.PrismEclipse = Minor(make, "omen_prism_eclipse", "Prism Eclipse",
                "Candles: drops gain rarity; the aliens hit harder.", OmenSuit.Candles,
                "One drop per run is upgraded to Legendary.",
                Benefit(OmenEffectKind.RarityRolls, ScalingRule.Count, 1f),
                Handicap(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 10f));

            c.NewCards = new[]
            {
                c.IronVow, c.HungryHive, c.CullingLeaper, c.IonStorm, c.BrittleWelds, c.IronChitin, c.EngineeringDeck,
                c.TheFortress, c.TheWatcher, c.LongNight, c.SecondBreach, c.CullingSpitter, c.ChampionTide, c.BloodPrice
            };
        }

        private static OmenCardDefinition Minor(DefinitionMaker make, string id, string name, string description, OmenSuit suit,
            string legendaryTwist, params OmenEffect[] effects)
        {
            var card = make.Def<OmenCardDefinition>(id, Folder, name, description, "omen", "minor", suit.ToString().ToLowerInvariant());
            card.family = OmenFamily.Suit;
            card.suit = suit;
            card.rarity = Rarity.Common;
            card.commonThreat = 1;
            card.effects = effects;
            card.legendaryTwist = legendaryTwist;
            return card;
        }

        /// <summary>
        /// The Minor Arcana: one card per reward stream, each with a small handicap (+1 Threat and one enemy buff).
        /// The ledger's choice in mission 3 hands out the first: Coins (sold to the Lane), Seals (returned to Doramin),
        /// Lanterns (kept). Gleaner's Moon (Cogs) is mission 2's.
        /// </summary>
        private static void BuildMinorArcana(OmenRolloutContent c, DefinitionMaker make)
        {
            c.TheTithe = Minor(make, "omen_the_tithe", "The Tithe", "Coins: +20% contract pay; the aliens hit a little harder.", OmenSuit.Coins,
                "Pay is doubled if no unit is Wrecked.",
                Benefit(OmenEffectKind.ContractPayPercent, ScalingRule.Linear, 20f),
                Handicap(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 5f));

            c.BrokersSeal = Minor(make, "omen_brokers_seal", "The Broker's Seal", "Seals: +50% standing from the contract; the hive notices sooner.", OmenSuit.Seals,
                "Standing can't drop this run.",
                Benefit(OmenEffectKind.StandingPercent, ScalingRule.Linear, 50f),
                Handicap(OmenEffectKind.AwarenessJumpPercent, ScalingRule.Linear, 10f));

            c.TheLamplighter = Minor(make, "omen_the_lamplighter", "The Lamplighter", "Lanterns: +1 lead to a charted hulk and 20% of the map revealed; tougher aliens.", OmenSuit.Lanterns,
                "The whole map is revealed, caches included.",
                Benefit(OmenEffectKind.IntelLeads, ScalingRule.Count, 1f),
                Benefit(OmenEffectKind.MapRevealFraction, ScalingRule.Chance, 0.2f),
                Handicap(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 5f));

            c.TheVeteran = Minor(make, "omen_the_veteran", "The Veteran", "Skulls: +20% commander and specialization XP; champions a little more common.", OmenSuit.Skulls,
                "Every specialization in the crew gains a level's worth of XP.",
                Benefit(OmenEffectKind.CommanderXpPercent, ScalingRule.Linear, 20f),
                Handicap(OmenEffectKind.ChampionMultiplier, ScalingRule.Multiplier, 1.1f));

            c.TheVigil = Minor(make, "omen_the_vigil", "The Vigil", "Candles: +1 card-or-Catalyst roll at Requisition; the hive stirs a minute sooner.", OmenSuit.Candles,
                "Every roll at Requisition is at least Uncommon.",
                Benefit(OmenEffectKind.DropRolls, ScalingRule.Count, 1f),
                Handicap(OmenEffectKind.HiveClockSeconds, ScalingRule.Linear, -60f));

            c.MinorCards = new[] { c.TheTithe, c.BrokersSeal, c.TheLamplighter, c.TheVeteran, c.TheVigil, c.TheDeck, c.PrismEclipse };
        }

        /// <summary>Our answer to Martyr's endgame card: a high Threat floor, no Commons, more of the best. Act 3, from Rank 12.</summary>
        private static void BuildAnchorite(OmenRolloutContent c, DefinitionMaker make)
        {
            var anchorite = Card(make, "omen_the_anchorite", "The Anchorite",
                "Some go into the dark to be found by it. Threat at least 12; no Common drops; Rare and better far more likely.",
                OmenFamily.Fortune, Rarity.Common, 0,
                "One Reliquary item is guaranteed Epic and carries the hulk's Condition affix.",
                Handicap(OmenEffectKind.ThreatFloor, ScalingRule.Linear, 12f),
                Handicap(OmenEffectKind.NoCommonDrops, ScalingRule.None, 1f),
                Benefit(OmenEffectKind.RareDropChancePercent, ScalingRule.Linear, 50f));
            c.TheAnchorite = anchorite;
        }

        private static void BuildCartomancer(OmenRolloutContent c, DefinitionMaker make)
        {
            var table = make.Def<CartomancerDefinition>("cartomancer_patusan", Folder, "The Cartomancer's table",
                "Levels cards from duplicates, ascends Level V cards, and mutates cards with Catalysts.", "omen", "cartomancer");
            table.copiesPerLevel = new[] { 1, 2, 3, 4 };
            table.levelFees = new[] { 25, 50, 100, 200 };
            table.trustedDiscountFromLevel = 3;
            table.ascendFees = new[] { 50, 150, 400, 1000 };
            table.corruptionWeights = new[] { 1f, 1f, 1f, 1f };

            OmenEffect H(OmenEffectKind kind, ScalingRule rule, float value) => Handicap(kind, rule, value);
            table.handicapPools = new[]
            {
                new OmenHandicapPool(OmenFamily.Hive, H(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 20f), H(OmenEffectKind.ChampionMultiplier, ScalingRule.Multiplier, 1.3f), H(OmenEffectKind.HiveClockSeconds, ScalingRule.Linear, -90f)),
                new OmenHandicapPool(OmenFamily.Ship, H(OmenEffectKind.WeldDurationPercent, ScalingRule.Linear, -30f), H(OmenEffectKind.HiveClockSeconds, ScalingRule.Linear, -120f), H(OmenEffectKind.AwarenessJumpPercent, ScalingRule.Linear, 15f)),
                new OmenHandicapPool(OmenFamily.Condition, H(OmenEffectKind.ConditionIntensity, ScalingRule.Linear, 1f), H(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 15f)),
                new OmenHandicapPool(OmenFamily.Fabrication, H(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 15f), H(OmenEffectKind.WeldDurationPercent, ScalingRule.Linear, -25f)),
                new OmenHandicapPool(OmenFamily.Expedition, H(OmenEffectKind.LootPercent, ScalingRule.Linear, -20f), H(OmenEffectKind.ContractPayPercent, ScalingRule.Linear, -20f)),
                new OmenHandicapPool(OmenFamily.Fortune, H(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 15f), H(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 15f), H(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 15f)),
                new OmenHandicapPool(OmenFamily.Culling, H(OmenEffectKind.LootPercent, ScalingRule.Linear, -15f), H(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 10f)),
                new OmenHandicapPool(OmenFamily.Brutality, H(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 20f), H(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 20f), H(OmenEffectKind.ChampionMultiplier, ScalingRule.Multiplier, 1.5f)),
                new OmenHandicapPool(OmenFamily.Hybrid, H(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 25f), H(OmenEffectKind.AwarenessJumpPercent, ScalingRule.Linear, 20f)),
                new OmenHandicapPool(OmenFamily.Vow, H(OmenEffectKind.AwarenessJumpPercent, ScalingRule.Linear, 20f), H(OmenEffectKind.HiveClockSeconds, ScalingRule.Linear, -90f)),
                new OmenHandicapPool(OmenFamily.Suit, H(OmenEffectKind.AlienDamagePercent, ScalingRule.Linear, 5f), H(OmenEffectKind.AlienHealthPercent, ScalingRule.Linear, 5f), H(OmenEffectKind.SurgeSizePercent, ScalingRule.Linear, 10f))
            };
            c.Cartomancer = table;
        }

        private static MissionOmenSchedule Row(int mission, string title, int slots, int slotsAfter, int dropsMin, int dropsMax,
            Rarity dropCeiling, bool cartomancer, Rarity ascendCeiling, int hand) =>
            new MissionOmenSchedule
            {
                mission = mission,
                workingTitle = title,
                slots = slots,
                slotsAfter = slotsAfter,
                dropsMin = dropsMin,
                dropsMax = dropsMax,
                dropCeiling = dropCeiling,
                cartomancerOpen = cartomancer,
                ascendCeiling = ascendCeiling,
                countermeasureHand = hand,
                benchOpen = mission >= BenchOpensAtMission,
                mutationsOpen = mission >= MutationsOpenAtMission,
                corruptionOpen = mission >= InfusionMath.OpensAtMission,
                hasSetPieceLockingMajor = mission == 8 || mission == 10
            };

        /// <summary>Catalysts on owned gear: from mission 4 (the T2 fabricator arrives in mission 3).</summary>
        public const int BenchOpensAtMission = CatalystCatalogBuilder.BenchCatalystsFromMission;

        /// <summary>Card mutations (Reroll, Deepen, Refine) at the Cartomancer's table.</summary>
        public const int MutationsOpenAtMission = 7;

        private static void BuildRollout(OmenRolloutContent c, DefinitionMaker make)
        {
            var t = c.Previous;
            var cs = t.Previous;

            var rollout = make.Def<OmenRolloutDefinition>(RolloutId, Folder, "Tarot rollout: missions 1–10",
                "When each Omen card, slot, rarity and the Cartomancer's table arrive.", "omen", "rollout");

            var sephora = Row(8, "The Sephora", 3, 3, 2, 3, Rarity.Rare, true, Rarity.Rare, 6);
            sephora.guaranteesRarity = true;
            sephora.guaranteedRarity = Rarity.Rare;

            rollout.schedule = new[]
            {
                Row(1, "Prologue", 0, 0, 0, 0, Rarity.Common, false, Rarity.Common, 1),
                Row(2, "Cold Sleep", 1, 1, 0, 0, Rarity.Common, false, Rarity.Common, 3),
                Row(3, "The Tremolino", 1, 2, 0, 0, Rarity.Common, false, Rarity.Common, 4),
                Row(4, "Pod 7", 2, 2, 1, 1, Rarity.Common, false, Rarity.Common, 4),
                Row(5, "The Nan-Shan", 2, 2, 2, 2, Rarity.Uncommon, false, Rarity.Common, 5),
                Row(6, "The ledger's price", 3, 3, 2, 2, Rarity.Uncommon, true, Rarity.Rare, 5),
                Row(7, "Old patterns", 3, 3, 2, 3, Rarity.Rare, true, Rarity.Rare, 6),
                sephora,
                Row(9, "Second breach", 3, 3, 2, 4, Rarity.Rare, true, Rarity.Rare, 7),
                Row(10, "The Ledger", 3, 3, 2, 4, Rarity.Epic, true, Rarity.Epic, 7)
            };

            // Cards found or won in a mission are slotted from the next one; the reader's free reading is slotted at once.
            rollout.entries = new List<OmenRolloutEntry>
            {
                // Gleaner's Moon is a Minor (Cogs): playable once the first Minor slot opens, after mission 3.
                new OmenRolloutEntry(1, cs.GleanersMoon, OmenSource.FaceDown, 4),

                new OmenRolloutEntry(2, cs.ThinSwarm, OmenSource.Reading),
                new OmenRolloutEntry(2, cs.ThickHide, OmenSource.Reading),
                new OmenRolloutEntry(2, cs.TheGhost, OmenSource.Reading),
                new OmenRolloutEntry(2, cs.ThinSwarmUncommon, OmenSource.Found, 3),

                new OmenRolloutEntry(3, t.SolarFlare, OmenSource.Found, 4),
                new OmenRolloutEntry(3, t.QuietDecks, OmenSource.Found, 4),
                new OmenRolloutEntry(3, c.TheTithe, OmenSource.Reward, 4, LedgerSoldFlag),
                new OmenRolloutEntry(3, c.BrokersSeal, OmenSource.Reward, 4, LedgerReturnedFlag),
                new OmenRolloutEntry(3, c.TheLamplighter, OmenSource.Reward, 4, LedgerKeptFlag),

                new OmenRolloutEntry(4, c.IronVow, OmenSource.Reward, 5),
                new OmenRolloutEntry(4, c.HungryHive, OmenSource.Found, 5),

                new OmenRolloutEntry(5, c.CullingLeaper, OmenSource.Reward, 6),
                new OmenRolloutEntry(5, c.IonStorm, OmenSource.Found, 6),
                new OmenRolloutEntry(5, c.TheVeteran, OmenSource.Found, 6),

                new OmenRolloutEntry(6, c.BrittleWelds, OmenSource.Reward, 7),
                new OmenRolloutEntry(6, c.TheDeck, OmenSource.Reward, 7, LedgerReturnedFlag, LedgerKeptFlag),
                new OmenRolloutEntry(6, c.IronChitin, OmenSource.Reward, 7, LedgerSoldFlag),

                new OmenRolloutEntry(7, c.EngineeringDeck, OmenSource.Reward, 8),
                new OmenRolloutEntry(7, c.TheFortress, OmenSource.Found, 8),
                new OmenRolloutEntry(7, c.TheVigil, OmenSource.Found, 8),

                new OmenRolloutEntry(8, c.TheWatcher, OmenSource.Reward, 9),
                new OmenRolloutEntry(8, c.LongNight, OmenSource.Found, 9),

                new OmenRolloutEntry(9, c.SecondBreach, OmenSource.Reward, 10),
                new OmenRolloutEntry(9, c.CullingSpitter, OmenSource.Found, 10),
                new OmenRolloutEntry(9, c.ChampionTide, OmenSource.Found, 10),

                new OmenRolloutEntry(10, c.BloodPrice, OmenSource.Reward, 11),
                new OmenRolloutEntry(10, c.PrismEclipse, OmenSource.Found, 11)
            }.ToArray();

            rollout.thirdSlotRank = 7;
            rollout.fourthSlotNode = "Reliquarian";
            rollout.mercyCard = cs.ThinSwarm;
            rollout.mercyAfterFailures = 2;
            rollout.mercyFromMission = 2;
            c.Rollout = rollout;
        }
    }
}
