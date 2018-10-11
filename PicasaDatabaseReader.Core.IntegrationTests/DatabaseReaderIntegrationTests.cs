using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Fields;
using PicasaDatabaseReader.Core.Tests;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.IntegrationTests
{
    public class DatabaseReaderIntegrationTests : TestsBase<DatabaseReaderIntegrationTests>
    {

        public DatabaseReaderIntegrationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected internal const string PathToDatabase = "C:\\Users\\Spade\\AppData\\Local\\Google\\Picasa2\\db3";

        [Fact]
        public async Task ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var fileSystem = new FileSystem();

            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            var tableNames = await databaseReader
                .GetTableNames()
                .ToArray()
                .FirstAsync();

            tableNames.Should().HaveCount(3);
        }

        [Theory]
        [InlineData("albumdata", 20)]
        [InlineData("catdata", 3)]
        [InlineData("imagedata", 36)]
        public async Task ShouldGetFieldFiles(string tableName, int fieldCount)
        {
            Logger.LogInformation("ShouldGetFieldFiles TableName:{TableName} FieldCount:{FieldCount}", tableName, fieldCount);

            var fileSystem = new FileSystem();
            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            var fields = await databaseReader
                .GetFieldFilePaths(tableName)
                .ToArray()
                .FirstAsync();

            fields.Should().HaveCount(fieldCount);
        }

        [Theory]
        [InlineData("albumdata")]
        [InlineData("catdata")]
        [InlineData("imagedata")]
        public async Task ShouldGetFields(string tableName)
        {
            Logger.LogInformation("ShouldGetFields {TableName}", tableName);

            var fileSystem = new FileSystem();
            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            var coreFields =
                await databaseReader
                    .GetFields(tableName).ToArray().FirstAsync();

            var legacyFields =
                await databaseReader
                    .GetFieldFilePaths(tableName)
                    .Select(PicasaDatabaseReader.FieldFactory.CreateField).ToArray().FirstAsync();

            var actual = coreFields
                .Select(field => new
                {
                    name = field.Name,
                    type = field.Type,
                    count = field.Count
                })
                .OrderBy(arg => arg.name)
                .ToArray();

            var expected = legacyFields
                .Select(field => new
                {
                    name = field.Name,
                    type = field.Type,
                    count = field.Count
                })
                .OrderBy(arg => arg.name)
                .ToArray();

            actual
                .Should()
                .BeEquivalentTo(expected);
        }
    }
}
