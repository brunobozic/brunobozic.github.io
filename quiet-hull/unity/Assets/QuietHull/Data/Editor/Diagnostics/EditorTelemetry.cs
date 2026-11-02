using System;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Logging and tracing for the editor data tools.
    /// <list type="bullet">
    /// <item>Serilog: structured events enriched with editor context and the current trace and span ids,
    /// so a Graylog search can jump to the matching Jaeger trace.</item>
    /// <item>OpenTelemetry: spans come from <see cref="Source"/> (System.Diagnostics.ActivitySource, the .NET
    /// OTEL API). Any OTEL SDK listener the project registers exports them; without one,
    /// <see cref="SpanLogListener"/> writes each finished span to the log with its duration and tags.</item>
    /// </list>
    /// Everything is created lazily and published thread-safely, so the tools can log from any thread.
    /// </summary>
    public static class EditorTelemetry
    {
        public const string ServiceName = "QuietHull.Editor";
        public const string SourceName = "QuietHull.Editor.Data";
        public const string SourceVersion = "1.0.0";

        public static readonly ActivitySource Source = new ActivitySource(SourceName, SourceVersion);

        private static readonly Lazy<ILogger> LazyLogger = new Lazy<ILogger>(CreateLogger, LazyThreadSafetyMode.ExecutionAndPublication);
        private static int _listenerRegistered;

        /// <summary>The root logger. Use <c>ForContext</c> for per-tool context.</summary>
        public static ILogger Logger
        {
            get
            {
                EnsureSpanListener();
                return LazyLogger.Value;
            }
        }

        public static ILogger For<T>() => Logger.ForContext<T>();

        /// <summary>Sets the standard tags every definition span carries.</summary>
        public static Activity TagDefinition(this Activity activity, DefinitionBase definition, string assetPath = null)
        {
            if (activity == null || definition == null)
            {
                return activity;
            }

            activity.SetTag("quiethull.definition.id", definition.Id);
            activity.SetTag("quiethull.definition.type", definition.GetType().Name);
            activity.SetTag("quiethull.definition.display_name", definition.DisplayName);
            if (!string.IsNullOrEmpty(assetPath))
            {
                activity.SetTag("quiethull.asset.path", assetPath);
            }

            return activity;
        }

        /// <summary>Marks an activity as failed with the exception details (OTEL semantic conventions).</summary>
        public static void RecordException(this Activity activity, Exception exception)
        {
            if (activity == null || exception == null)
            {
                return;
            }

            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.SetTag("exception.type", exception.GetType().FullName);
            activity.SetTag("exception.message", exception.Message);
            activity.SetTag("exception.stacktrace", exception.StackTrace);
        }

        private static ILogger CreateLogger()
        {
            // Application.* must be read on the main thread; the first log call comes from a menu item.
            var unityVersion = Application.unityVersion;
            var productName = Application.productName;
            var platform = Application.platform.ToString();

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", ServiceName)
                .Enrich.WithProperty("UnityVersion", unityVersion)
                .Enrich.WithProperty("ProductName", productName)
                .Enrich.WithProperty("Platform", platform)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ProcessId", Process.GetCurrentProcess().Id)
                .Enrich.With<ActivityEnricher>()
                .Enrich.With<ThreadEnricher>()
                .WriteTo.Sink(new UnityConsoleSink())
                // Graylog: add Serilog.Sinks.Graylog and .WriteTo.Graylog(...) here once the
                // shared telemetry bootstrap (game + editor) is in place.
                .CreateLogger();
        }

        private static void EnsureSpanListener()
        {
            if (Interlocked.Exchange(ref _listenerRegistered, 1) == 0)
            {
                SpanLogListener.Register(LazyLogger);
            }
        }

        /// <summary>Adds TraceId, SpanId and ParentSpanId from the current activity.</summary>
        private sealed class ActivityEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                var activity = Activity.Current;
                if (activity == null)
                {
                    return;
                }

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToHexString()));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToHexString()));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ParentSpanId", activity.ParentSpanId.ToHexString()));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Operation", activity.OperationName));
            }
        }

        private sealed class ThreadEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Environment.CurrentManagedThreadId));
            }
        }
    }
}
