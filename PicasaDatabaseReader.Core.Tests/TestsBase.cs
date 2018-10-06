using System;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests
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

    public abstract class TestsBase<T>
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly ILogger<T> Logger;

        public TestsBase(ITestOutputHelper testOutputHelper)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithThreadId()
                .Enrich.With<CustomEnrichers>()
                .WriteTo.TestOutput(testOutputHelper, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] ({PaddedThreadId}) {ShortSourceContext} {Message}{NewLineIfException}{Exception}")
                .CreateLogger();

            var services = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog());

            ServiceProvider = services.BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILogger<T>>();
        }
    }

    public abstract class UnitTestsBase<T> : TestsBase<T>
    {
        protected readonly Faker Faker;

        protected UnitTestsBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Faker = new Faker();
        }
    }
}