using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Scheduling;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PicasaDatabaseReader.Core.Tests
{
    public class DatabaseReaderTests : UnitTestsBase<DatabaseReaderTests>
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();

        public DatabaseReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"{name}_0";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(BitConverter.GetBytes(DatabaseReader.TableFileHeader));
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);

            var tableNames = databaseReader
                .GetTableNames();

            var autoResetEvent = new AutoResetEvent(false);

            tableNames.Subscribe(_ => { }, () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact]
        public async Task ShouldGetFieldFiles()
        {
            Logger.LogInformation("ShouldGetFieldFiles");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"TestTable_{name}.pmp";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(string.Empty);
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);

            var tableNames = await databaseReader
                .GetFieldFilePaths("TestTable")
                .ToArray()
                .FirstAsync();

            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.path));
        }
    }
}
