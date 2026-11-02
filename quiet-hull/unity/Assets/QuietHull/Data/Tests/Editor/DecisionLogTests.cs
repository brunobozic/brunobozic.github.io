using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using QuietHull.Data.Editor;

namespace QuietHull.Data.Tests
{
    /// <summary>The decision log and the company's forecast file (the "Theme: the guided monster" tab).</summary>
    public sealed class DecisionLogTests
    {
        private InMemoryDefinitionFactory _factory;
        private StoryDecisions _d;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryDefinitionFactory();
            _d = StoryDecisionsBuilder.Build(_factory);
        }

        [TearDown]
        public void TearDown() => _factory.Dispose();

        private sealed class Spy : IDecisionLogObserver
        {
            public int Calls;
            public DecisionRecord Last;

            public void OnRecorded(DecisionRecord record)
            {
                Calls++;
                Last = record;
            }
        }

        [Test]
        public void Catalog_HasNoValidationErrors()
        {
            var errors = _d.Catalog.Validate().Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.ToString()).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void TheLedgerForecast_MatchesTheStoryChoice()
        {
            Assert.That(_d.Ledger.StoryChoice, Is.SameAs(_d.Previous.Ledger));
            Assert.That(_d.Ledger.Options.Select(o => o.optionId), Is.EquivalentTo(_d.Previous.Ledger.Options.Select(o => o.optionId)));
        }

        [Test]
        public void Verdicts_ExpectedDeviationAndUnforecast()
        {
            Assert.That(_d.PatusanRelease.Classify("open"), Is.EqualTo(ForecastVerdict.Expected));
            Assert.That(_d.PatusanRelease.Classify("controlled"), Is.EqualTo(ForecastVerdict.Deviation));
            Assert.That(_d.Finale.Classify(StoryDecisionsBuilder.StayOptionId), Is.EqualTo(ForecastVerdict.Unforecast));
        }

        [Test]
        public void TheFinale_HasExactlyOneUnforecastChoice_AndExpectsTheLaunch()
        {
            Assert.That(_d.Finale.Options.Count(o => o.unforecast), Is.EqualTo(1));
            Assert.That(_d.Finale.ModalOption.optionId, Is.EqualTo("launch"));
            Assert.That(_d.Finale.ModalOption.predicted, Is.GreaterThanOrEqualTo(0.95f));
            Assert.That(_d.AllDecisions.Where(d => d != _d.Finale).SelectMany(d => d.Options), Has.None.Matches<ForecastOption>(o => o.unforecast),
                "only the finale has a choice the company can't forecast");
        }

        [Test]
        public void Record_LogsInOrder_WithThePrediction_AndTellsObservers()
        {
            var log = new DecisionLog();
            var spy = new Spy();
            log.AddObserver(spy);

            var first = log.Record(_d.Ledger, "keep");
            var second = log.Record(_d.PatusanRelease, "open");

            Assert.That(first.Sequence, Is.EqualTo(1));
            Assert.That(first.Predicted, Is.EqualTo(0.21f).Within(1e-6f));
            Assert.That(first.Verdict, Is.EqualTo(ForecastVerdict.Deviation));
            Assert.That(second.Sequence, Is.EqualTo(2));
            Assert.That(second.Mission, Is.EqualTo(6));
            Assert.That(spy.Calls, Is.EqualTo(2));
            Assert.That(spy.Last, Is.EqualTo(second));
            Assert.That(log.HasDecided("dp_ledger"), Is.True);
        }

        [Test]
        public void Record_EachDecisionOnce()
        {
            var log = new DecisionLog();
            log.Record(_d.Jones, "spaced");
            Assert.Throws<InvalidOperationException>(() => log.Record(_d.Jones, "cryo"));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => log.Record(_d.Cornelius, "forgiven"));
        }

        [Test]
        public void TheFile_AtMissionTen_ReadsTheChoicesBack_WithTheirOdds()
        {
            var log = new DecisionLog();
            log.Record(_d.Ledger, "keep");
            log.Record(_d.Autodoctor, "take");
            log.Record(_d.PatusanRelease, "open");
            log.Record(_d.Jones, "spaced");
            log.Record(_d.SplicesAtTransfer, "2");
            log.Record(_d.Finale, StoryDecisionsBuilder.StayOptionId);

            var file = ForecastReport.Build(log, _d.AllDecisions, StoryDecisionsBuilder.RevealMission);
            var text = file.Lines.Select(l => l.Text).ToList();

            Assert.That(text, Does.Contain("Ledger: retained. Predicted 0.21. Acceptable deviation."));
            Assert.That(text, Does.Contain("Patusan: release via ventilation (Open). Predicted 0.67."));
            Assert.That(text, Does.Contain("Jones: spaced. Predicted 0.52."));
            Assert.That(text, Has.Count.EqualTo(5), "the finale isn't in the file yet");
            Assert.That(file.ExpectedCount, Is.EqualTo(3));
            Assert.That(file.DeviationCount, Is.EqualTo(2));

            var end = ForecastReport.Build(log, _d.AllDecisions, 18);
            Assert.That(end.Lines.Last().Text, Is.EqualTo("Final disposition: candidate stays aboard. Predicted —."));
            Assert.That(end.UnforecastCount, Is.EqualTo(1));
        }

        [Test]
        public void TheLog_SurvivesASave()
        {
            var log = new DecisionLog();
            log.Record(_d.Ledger, "lane");
            log.Record(_d.Cornelius, "taken");

            var loaded = DecisionLog.FromState(log.ToState());
            Assert.That(loaded.Records, Is.EqualTo(log.Records));
            Assert.Throws<InvalidOperationException>(() => loaded.Record(_d.Ledger, "keep"), "a loaded log still refuses a second answer");
        }

        [Test]
        public void TheLog_IsSafeUnderConcurrentWrites()
        {
            var log = new DecisionLog();
            Parallel.ForEach(_d.AllDecisions, d => log.Record(d, d.Options[0].optionId));

            Assert.That(log.Count, Is.EqualTo(_d.AllDecisions.Count));
            Assert.That(log.Records.Select(r => r.Sequence), Is.EquivalentTo(Enumerable.Range(1, _d.AllDecisions.Count)));
            Assert.That(log.Records.Select(r => r.DecisionId).Distinct().Count(), Is.EqualTo(_d.AllDecisions.Count));
        }

        private sealed class ListSink : Serilog.Core.ILogEventSink
        {
            public readonly System.Collections.Concurrent.ConcurrentQueue<Serilog.Events.LogEvent> Events =
                new System.Collections.Concurrent.ConcurrentQueue<Serilog.Events.LogEvent>();

            public void Emit(Serilog.Events.LogEvent logEvent) => Events.Enqueue(logEvent);
        }

        [Test]
        public void Telemetry_LogsEachDecision_WithItsContext_AndWarnsOnTheUnforecastOne()
        {
            var sink = new ListSink();
            var logger = new Serilog.LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(sink).CreateLogger();
            using (var source = new System.Diagnostics.ActivitySource("QuietHull.Tests.Decisions"))
            {
                var log = new DecisionLog();
                log.AddObserver(new TelemetryDecisionLogObserver(logger, source));
                log.Record(_d.PatusanRelease, "controlled");
                log.Record(_d.Finale, StoryDecisionsBuilder.StayOptionId);
            }

            var events = sink.Events.ToArray();
            Assert.That(events, Has.Length.EqualTo(2));
            Assert.That(events[0].Level, Is.EqualTo(Serilog.Events.LogEventLevel.Information));
            Assert.That(events[0].Properties["DecisionId"].ToString(), Does.Contain("dp_m6_release"));
            Assert.That(events[0].Properties["Verdict"].ToString(), Does.Contain("Deviation"));
            Assert.That(events[1].Level, Is.EqualTo(Serilog.Events.LogEventLevel.Warning));
            Assert.That(events[1].Properties["OptionId"].ToString(), Does.Contain(StoryDecisionsBuilder.StayOptionId));
        }

        [Test]
        public void Validation_CatchesABadForecast()
        {
            var bad = _factory.GetOrCreate<DecisionPointDefinition>("dp_bad", "Story/Decisions");
            bad.id = "dp_bad";
            bad.displayName = "Bad";
            bad.fileLabel = "Bad";
            bad.options = new[]
            {
                new ForecastOption("a", "A", "a", 0.5f),
                new ForecastOption("a", "A again", "a", 0.3f)
            };

            var report = new ValidationReport();
            bad.Validate(report);
            var errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.Message).ToList();
            Assert.That(errors, Has.Some.Contains("duplicated"));
            Assert.That(errors, Has.Some.Contains("add up to 0.8"));
        }
    }
}
