using System.Linq;
using NUnit.Framework;

namespace QuietHull.Data.Tests
{
    /// <summary>The rarity matrix from the design doc (Content lists ▸ Rarity math), checked row by row.</summary>
    public sealed class RarityMathTests
    {
        private static readonly Rarity[] All = { Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Epic, Rarity.Legendary };

        private static int[] Percent(float common, ScalingRule rule, OmenSide side) =>
            All.Select(r => RarityMath.RoundPercent(RarityMath.Scale(common, rule, RarityMath.Factor(side, r)))).ToArray();

        private static int[] PercentOfFraction(float common, ScalingRule rule, OmenSide side) =>
            All.Select(r => RarityMath.RoundPercent(100f * RarityMath.Scale(common, rule, RarityMath.Factor(side, r)))).ToArray();

        [TestCase(2, new[] { 2, 3, 3, 4, 4 })]
        [TestCase(3, new[] { 3, 4, 5, 5, 6 })]
        [TestCase(4, new[] { 4, 5, 6, 7, 8 })]
        [TestCase(-3, new[] { -3, -4, -5, -5, -6 })]
        [TestCase(-1, new[] { -1, -1, -2, -2, -2 })]
        public void Threat_ScalesByHandicapAndRoundsAwayFromZero(int common, int[] expected)
        {
            Assert.That(All.Select(r => RarityMath.ScaleThreat(common, r)), Is.EqualTo(expected));
        }

        [Test]
        public void HungryHive_Handicap_25Percent() =>
            Assert.That(Percent(25f, ScalingRule.Linear, OmenSide.Handicap), Is.EqualTo(new[] { 25, 31, 38, 44, 50 }));

        [Test]
        public void PercentBenefit_50Percent() =>
            Assert.That(Percent(50f, ScalingRule.Linear, OmenSide.Benefit), Is.EqualTo(new[] { 50, 75, 100, 125, 150 }));

        [Test]
        public void FocusBias_40PercentChance() =>
            Assert.That(PercentOfFraction(0.4f, ScalingRule.Chance, OmenSide.Benefit), Is.EqualTo(new[] { 40, 54, 64, 72, 78 }));

        [Test]
        public void AffixBias_30PercentChance() =>
            Assert.That(PercentOfFraction(0.3f, ScalingRule.Chance, OmenSide.Benefit), Is.EqualTo(new[] { 30, 41, 51, 59, 66 }));

        [Test]
        public void Reduction_50Percent() =>
            Assert.That(PercentOfFraction(0.5f, ScalingRule.Reduction, OmenSide.Benefit), Is.EqualTo(new[] { 50, 65, 75, 82, 88 }));

        [Test]
        public void HandicapReduction_WeldsHold() =>
            // Brittle Welds: welds hold 50 / 42 / 35 / 30 / 25% as long, i.e. reduced by 50 / 58 / 65 / 70 / 75%.
            Assert.That(PercentOfFraction(0.5f, ScalingRule.Reduction, OmenSide.Handicap), Is.EqualTo(new[] { 50, 58, 65, 70, 75 }));

        [Test]
        public void Multiplier_X2() =>
            Assert.That(All.Select(r => RarityMath.Scale(2f, ScalingRule.Multiplier, RarityMath.M(r))), Is.EqualTo(new[] { 2f, 2.5f, 3f, 3.5f, 4f }));

        [Test]
        public void Structural_Floors() =>
            Assert.That(All.Select(r => RarityMath.Scale(1f, ScalingRule.Structural, RarityMath.M(r))), Is.EqualTo(new[] { 1f, 1f, 2f, 2f, 3f }));

        [Test]
        public void BenefitGrowsFasterThanHandicap()
        {
            foreach (var rarity in All)
            {
                Assert.That(RarityMath.M(rarity), Is.GreaterThanOrEqualTo(RarityMath.H(rarity)), rarity.ToString());
            }
        }

        [Test]
        public void StatMath_SumsPercentagesAfterFlatAdds()
        {
            var modifiers = new[]
            {
                StatModifier.Add(UnitStat.Armour, 15f),
                StatModifier.Add(UnitStat.Armour, 30f),
                StatModifier.Percent(UnitStat.Armour, 0.1f),
                StatModifier.Percent(UnitStat.Armour, 0.1f),
                StatModifier.Percent(UnitStat.MoveSpeed, 0.5f)
            };

            Assert.That(StatMath.Apply(UnitStat.Armour, 15f, modifiers), Is.EqualTo(60f * 1.2f).Within(1e-4f));
            Assert.That(StatMath.Apply(UnitStat.CritChance, 0.05f, modifiers), Is.EqualTo(0.05f));
        }
    }
}
