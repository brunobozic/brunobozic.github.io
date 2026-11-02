using System.Linq;
using NUnit.Framework;
using QuietHull.Data.Editor;
using UnityEngine;

namespace QuietHull.Data.Tests
{
    /// <summary>Checks that validation actually catches broken data.</summary>
    public sealed class ValidationTests
    {
        private InMemoryDefinitionFactory _factory;

        [SetUp]
        public void SetUp() => _factory = new InMemoryDefinitionFactory();

        [TearDown]
        public void TearDown() => _factory.Dispose();

        [Test]
        public void Id_MustBeLowerSnakeCase()
        {
            var prod = _factory.GetOrCreate<MeleeWeaponDefinition>("itm_x", "Items");
            prod.id = "Shock Prod";
            prod.displayName = "Shock prod";

            var report = new ValidationReport();
            prod.Validate(report);

            Assert.That(report.Issues.Any(i => i.Field == "id" && i.Severity == ValidationSeverity.Error), Is.True);
        }

        [Test]
        public void Catalog_ReportsDuplicateIds()
        {
            var a = _factory.GetOrCreate<MaterialDefinition>("mat_a", "Materials");
            var b = _factory.GetOrCreate<MaterialDefinition>("mat_b", "Materials");
            a.id = "mat_same";
            b.id = "mat_same";
            a.displayName = b.displayName = "Same";

            var catalog = _factory.GetOrCreateCatalog("cat_test");
            catalog.definitions = new System.Collections.Generic.List<DefinitionBase> { a, b };

            var report = catalog.Validate();

            Assert.That(report.Issues.Any(i => i.Message.Contains("duplicated")), Is.True);
        }

        [Test]
        public void Cargo_NeedsFourHeatEntries()
        {
            var cell = _factory.GetOrCreate<CargoDefinition>("itm_cell", "Items");
            cell.id = "itm_cell";
            cell.displayName = "Cell";
            cell.conditionLossPerSecondByHeat = new[] { 0f, 1f };

            var report = new ValidationReport();
            cell.Validate(report);

            Assert.That(report.HasErrors, Is.True);
        }

        [Test]
        public void Ambience_PeakMustBeShorterThanPeriod()
        {
            var room = _factory.GetOrCreate<RoomAmbienceDefinition>("amb_x", "World");
            room.id = "amb_x";
            room.displayName = "X";
            room.pattern = AmbiencePattern.Pulsing;
            room.periodSeconds = 2f;
            room.peakSeconds = 2f;
            room.causeText = "Test";

            var report = new ValidationReport();
            room.Validate(report);

            Assert.That(report.Issues.Any(i => i.Field == "peakSeconds"), Is.True);
        }

        [Test]
        public void ArmourMath_MatchesTheBalanceModel()
        {
            Assert.That(ArmourMath.DamageMultiplier(0f), Is.EqualTo(1f));
            Assert.That(ArmourMath.DamageMultiplier(100f), Is.EqualTo(0.5f));
            Assert.That(ArmourMath.DamageMultiplier(-50f), Is.EqualTo(1f));
        }

        [Test]
        public void NoiseMath_MaskingNeverGoesNegative()
        {
            Assert.That(NoiseMath.Masked(6f, 10f), Is.EqualTo(0f));
            Assert.That(NoiseMath.Masked(12f, 10f), Is.EqualTo(2f));
            Assert.That(NoiseMath.Masked(5f, -3f), Is.EqualTo(5f));
        }

        [Test]
        public void Catalog_LookupIsSafeUnderConcurrentReads()
        {
            var content = PrologueContentBuilder.Build(_factory);
            var catalog = content.Catalog;
            catalog.InvalidateLookup();

            var failures = 0;
            System.Threading.Tasks.Parallel.For(0, 2000, i =>
            {
                if (!catalog.TryGet<UnitDefinition>("unit_j1m", out var unit) || ReferenceEquals(unit, null))
                {
                    System.Threading.Interlocked.Increment(ref failures);
                }
            });

            Assert.That(failures, Is.Zero);
        }
    }
}
