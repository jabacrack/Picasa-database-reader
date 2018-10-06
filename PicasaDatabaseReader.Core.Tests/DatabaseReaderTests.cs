using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
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
        public void ShouldConstruct()
        {
            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Select(s => new { name = s, filename = s + "_0" })
                .Select(s => new { s.name, s.filename, path = Path.Combine(directoryPath, s.filename) })
                .ToArray();

            var mockFiles = args.ToDictionary(arg => arg.path, arg => new MockFileData(BitConverter.GetBytes(DatabaseReader.TableFileHeader)));

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = new DatabaseReader(mockFileSystem, directoryPath);
            var tableNames = databaseReader.GetTableNames();
            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.name));
        }
    }
}
