using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>Beacon, Sigil and Echo: Catalysts that steer which Omen card a drop becomes.</summary>
    public sealed class CardTargetingTests
    {
        private InMemoryDefinitionFactory _factory;
        private OmenRolloutContent _r;
        private List<OmenCardDefinition> _pool;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _r = OmenRolloutBuilder.Build(_factory);
            _pool = _r.Rollout.DropPool(10, f => f == OmenRolloutBuilder.LedgerSoldFlag);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        private float ChanceOf(OmenCardDefinition card, CardTargeting t) => CardDropMath.Chances(_pool, t).Single(p => p.Key == card).Value;

        [Test]
        public void TheCatalysts_AreMeta_AndWorkInTheReliquary()
        {
            foreach (var kind in new[] { CatalystKind.Beacon, CatalystKind.Sigil, CatalystKind.Echo })
            {
                Assert.That(_r.Catalysts[kind].IsMeta, Is.True, kind.ToString());
                Assert.That(_r.Catalysts[kind].Uses, Is.EqualTo(CatalystUse.Reliquary), kind.ToString());
            }
        }

        [Test]
        public void WithoutCatalysts_EveryMetCardIsEquallyLikely()
        {
            var chances = CardDropMath.Chances(_pool, CardTargeting.None);
            Assert.That(chances.Select(c => c.Value), Has.All.EqualTo(1f / _pool.Count).Within(1e-6f));
        }

        [Test]
        public void Beacon_Common_ClaimsAFifth_PlusItsUniformShare()
        {
            var target = _r.IronVow;
            var t = CardTargeting.None.WithBeacon(target, Rarity.Common);
            Assert.That(ChanceOf(target, t), Is.EqualTo(0.2f + 0.8f / _pool.Count).Within(1e-6f));
        }

        [Test]
        public void Beacon_RisesWithRarity()
        {
            var shares = Enum.GetValues(typeof(Rarity)).Cast<Rarity>().Select(r => ChanceOf(_r.IronVow, CardTargeting.None.WithBeacon(_r.IronVow, r))).ToList();
            Assert.That(shares, Is.Ordered.Ascending);
            Assert.That(CardDropMath.BeaconShare(Rarity.Legendary), Is.EqualTo(0.8f));
        }

        [Test]
        public void Beacon_OnACardNotYetMet_DoesNothing()
        {
            var t = CardTargeting.None.WithBeacon(_r.TheAnchorite, Rarity.Legendary);
            Assert.That(_pool, Has.No.Member(_r.TheAnchorite));
            Assert.That(CardDropMath.Chances(_pool, t).Select(c => c.Value), Has.All.EqualTo(1f / _pool.Count).Within(1e-6f));
        }

        [Test]
        public void Sigil_SteersDropsToAFamily()
        {
            var hive = _pool.Where(c => c.Family == OmenFamily.Hive).ToList();
            var t = CardTargeting.None.WithSigil(OmenFamily.Hive, Rarity.Common);
            var total = CardDropMath.Chances(_pool, t).Where(p => p.Key.Family == OmenFamily.Hive).Sum(p => p.Value);
            Assert.That(total, Is.EqualTo(0.35f + 0.65f * hive.Count / _pool.Count).Within(1e-5f));
        }

        [Test]
        public void Sigil_CanTargetASuit()
        {
            var t = CardTargeting.None.WithSigil(OmenSuit.Cogs, Rarity.Legendary);
            Assert.That(ChanceOf(_r.Previous.Previous.GleanersMoon, t), Is.EqualTo(1f).Within(1e-6f), "a Legendary Sigil on the only Cogs card");
        }

        [Test]
        public void Echo_FeedsTheSlottedCards()
        {
            var slotted = new[] { _r.Previous.Previous.ThickHide, _r.TheTithe };
            var t = CardTargeting.None.WithEcho(slotted, Rarity.Common);
            foreach (var card in slotted)
            {
                Assert.That(ChanceOf(card, t), Is.EqualTo(0.1f + 0.8f / _pool.Count).Within(1e-6f));
            }
        }

        [Test]
        public void BeaconAndEcho_NeverClaimMoreThanNinetyPercent()
        {
            var slotted = new[] { _r.Previous.Previous.ThickHide, _r.TheTithe, _r.Previous.Previous.GleanersMoon };
            var t = CardTargeting.None.WithBeacon(_r.IronVow, Rarity.Legendary).WithEcho(slotted, Rarity.Legendary);
            var chances = CardDropMath.Chances(_pool, t);
            var targeted = chances.Where(p => p.Key == _r.IronVow || slotted.Contains(p.Key)).Sum(p => p.Value);
            var others = _pool.Count - 4;
            Assert.That(targeted, Is.EqualTo(0.9f + 0.1f * 4 / _pool.Count).Within(1e-5f));
            Assert.That(chances.Where(p => p.Key != _r.IronVow && !slotted.Contains(p.Key)).Sum(p => p.Value), Is.EqualTo(0.1f * others / _pool.Count).Within(1e-5f));
        }

        [Test]
        public void Chances_AlwaysSumToOne()
        {
            var combos = new[]
            {
                CardTargeting.None,
                CardTargeting.None.WithBeacon(_r.IronVow, Rarity.Rare),
                CardTargeting.None.WithSigil(OmenFamily.Brutality, Rarity.Epic).WithBeacon(_r.HungryHive, Rarity.Uncommon),
                CardTargeting.None.WithEcho(new[] { _r.IronVow, _r.TheTithe }, Rarity.Epic).WithSigil(OmenSuit.Coins, Rarity.Rare)
                    .WithBeacon(_r.IronVow, Rarity.Legendary)
            };
            foreach (var t in combos)
            {
                Assert.That(CardDropMath.Chances(_pool, t).Sum(p => p.Value), Is.EqualTo(1f).Within(1e-5f));
            }
        }

        [Test]
        public void Resolve_FollowsTheChances()
        {
            var t = CardTargeting.None.WithBeacon(_pool[0], Rarity.Legendary);
            Assert.That(CardDropMath.Resolve(_pool, t, 0f), Is.SameAs(_pool[0]));
            Assert.That(CardDropMath.Resolve(_pool, t, 0.8f), Is.SameAs(_pool[0]), "0.8 + a uniform share");
            Assert.That(CardDropMath.Resolve(_pool, t, 0.9999f), Is.SameAs(_pool[_pool.Count - 1]));
            Assert.That(CardDropMath.Resolve(new List<OmenCardDefinition>(), t, 0.5f), Is.Null);
        }

        [Test]
        public void ACommonBeacon_TakesACardFromLevelOneToFive_InAboutFifteenRuns()
        {
            // 2–4 card drops per run by mission 10: 3 on average.
            var copiesPerRun = 3f * ChanceOf(_r.IronVow, CardTargeting.None.WithBeacon(_r.IronVow, Rarity.Common));
            var runs = 10f / copiesPerRun;
            Assert.That(runs, Is.InRange(12f, 16f), $"{runs:0.0} runs; without a Beacon about {10f / (3f / _pool.Count):0} runs");
        }
    }
}
