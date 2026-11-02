using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// Writes Serilog events to the Unity console. Debug.Log* is thread-safe in Unity, and the
    /// formatter is immutable, so the sink can be called from any thread without locking.
    /// </summary>
    public sealed class UnityConsoleSink : ILogEventSink
    {
        private const string DefaultTemplate = "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

        private readonly MessageTemplateTextFormatter _formatter;

        public UnityConsoleSink(string outputTemplate = DefaultTemplate)
        {
            _formatter = new MessageTemplateTextFormatter(outputTemplate ?? DefaultTemplate, null);
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            using (var writer = new StringWriter())
            {
                _formatter.Format(logEvent, writer);
                var text = writer.ToString().TrimEnd();

                switch (logEvent.Level)
                {
                    case LogEventLevel.Error:
                    case LogEventLevel.Fatal:
                        UnityEngine.Debug.LogError(text);
                        break;
                    case LogEventLevel.Warning:
                        UnityEngine.Debug.LogWarning(text);
                        break;
                    default:
                        UnityEngine.Debug.Log(text);
                        break;
                }
            }
        }
    }
}
