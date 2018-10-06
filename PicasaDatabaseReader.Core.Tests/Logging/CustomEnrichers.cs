using System;
using Serilog.Core;
using Serilog.Events;

namespace PicasaDatabaseReader.Core.Tests.Logging
{
    public class CustomEnrichers : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            AddShortSourceContext(logEvent);
            AddPaddedThreadId(logEvent);
            AddNewLineIfException(logEvent);
        }

        private static void AddShortSourceContext(LogEvent logEvent)
        {
            var logEventProperty = logEvent.Properties["SourceContext"];
            var propertyValue = logEventProperty.ToString();
            var start = propertyValue.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1;
            var end = propertyValue.Length - start - 1;
            var shortSourceContext = propertyValue.Substring(start, end);

            var eventProperty = new LogEventProperty("ShortSourceContext", new ScalarValue(shortSourceContext));
            logEvent.AddPropertyIfAbsent(eventProperty);
        }

        private static void AddPaddedThreadId(LogEvent logEvent)
        {
            var logEventProperty = logEvent.Properties["ThreadId"];
            var propertyValue = int.Parse(logEventProperty.ToString());
            var paddedThreadId = propertyValue.ToString().PadLeft(2, '0');

            var eventProperty = new LogEventProperty("PaddedThreadId", new ScalarValue(paddedThreadId));
            logEvent.AddPropertyIfAbsent(eventProperty);
        }

        private static void AddNewLineIfException(LogEvent logEvent)
        {
            var hasException = logEvent.Properties.ContainsKey("Exception");
            var result = hasException ? Environment.NewLine : string.Empty;

            var eventProperty = new LogEventProperty("NewLineIfException", new ScalarValue(result));
            logEvent.AddPropertyIfAbsent(eventProperty);
        }
    }
}