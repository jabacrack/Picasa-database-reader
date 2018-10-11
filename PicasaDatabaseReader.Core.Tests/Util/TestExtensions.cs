using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Scheduling;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public static class TestExtensions
    {
        public static DatabaseReader CreateDatabaseReader<T>(this TestsBase<T> databaseReaderTests,
            IFileSystem fileSystem,
            string directoryPath,
            ISchedulerProvider testScheduleProvider = null)
        {
            var serviceCollection = databaseReaderTests.GetServiceCollection()
                .AddSingleton(fileSystem)
                .AddScoped(provider => new DatabaseReader(provider.GetService<IFileSystem>(), directoryPath,
                    provider.GetService<ILogger<DatabaseReader>>(), provider.GetService<ISchedulerProvider>()));

            if (testScheduleProvider != null)
            {
                serviceCollection.AddSingleton(testScheduleProvider);
            }

            var buildServiceProvider = serviceCollection
                .BuildServiceProvider();

            return buildServiceProvider.GetService<DatabaseReader>();
        }
    }
}