using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests
{
    public class DatabaseReaderTests: UnitTestsBase<DatabaseReaderTests>
    {
        public DatabaseReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Select(name =>
                {
                    var filename = $"{name}_0";
                    var path = Path.Combine(directoryPath, filename);
                    return new {name, filename, path};
                })
                .ToArray();

            var mockFileData = new MockFileData(BitConverter.GetBytes(DatabaseReader.TableFileHeader));
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = new DatabaseReader(mockFileSystem, directoryPath, GetLogger<DatabaseReader>());
            var tableNames = await databaseReader
                .GetTableNames()
                .ToArray()
                .FirstAsync();

            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.name));
        }

        [Fact]
        public async Task ShouldGetFieldFiles()
        {
            Logger.LogInformation("ShouldGetFieldFiles");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Select(name =>
                {
                    var filename = $"TestTable_{name}.pmp";
                    var path = Path.Combine(directoryPath, filename);
                    return new {name, filename, path};
                })
                .ToArray();

            var mockFileData = new MockFileData(string.Empty);
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = new DatabaseReader(mockFileSystem, directoryPath, GetLogger<DatabaseReader>());
            var tableNames = await databaseReader
                .GetFieldFilePaths("TestTable")
                .ToArray()
                .FirstAsync();

            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.path));
        }
    }
}
