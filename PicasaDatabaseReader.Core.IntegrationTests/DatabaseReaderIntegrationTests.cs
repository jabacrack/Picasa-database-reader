using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> ShouldGetFieldDataCases() => 
            DbUtils
                .GetTablesNames(PathToDatabase)
                .SelectMany(tableName =>
                    DbUtils.GetFieldsFiles(PathToDatabase, tableName)
                        .Select(fieldFile =>
                        {
                            var fieldFileName = Path.GetFileNameWithoutExtension(fieldFile);
                            var fieldName = fieldFileName.Substring(fieldFileName.IndexOf("_") + 1);
                            return new object[] {tableName, fieldName};
                        }));

        [Theory]
        [MemberData(nameof(ShouldGetFieldDataCases))]
        public async Task ShouldGetFieldData(string tableName, string fieldName)
        {
            Logger.LogInformation("ShouldGetFieldData {TableName} {FieldFile}", tableName, fieldName);

            var fieldFileName = $"{tableName}_{fieldName}";

            var fileSystem = new FileSystem();
            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            var actual =
                await databaseReader
                    .GetFields(tableName)
                    .Where(field => field.Name == fieldName)
                    .Select(async field =>
                    {
                        var values = await field.GetValues().ToArray().FirstAsync();

                        return new { field, values };
                    })
                    .Concat()
                    .FirstAsync();

            var expected =
                await databaseReader
                    .GetFieldFilePaths(tableName)
                    .Where(s => Path.GetFileNameWithoutExtension(s) == fieldFileName)
                    .Select(fieldFilepath =>
                    {
                        var field = PicasaDatabaseReader.FieldFactory.CreateField(fieldFilepath);

                        var values = new object[field.Count];
                        for (int i = 0; i < field.Count; i++)
                        {
                            values[i] = field.ReadValue();
                        }

                        return new { field, values };
                    })
                    .FirstAsync();

            actual.field.Name.Should().Be(expected.field.Name);
            actual.field.Type.Should().Be(expected.field.Type);
            actual.field.Count.Should().Be(expected.field.Count);
            actual.values.Length.Should().Be(expected.values.Length);

            var body = actual.values
                .Zip(expected.values, (actualValue, expectedValue) => new { actualValue, expectedValue })
                .Select((arg, i) => new { arg.actualValue, arg.expectedValue, index = i });

            var selection = Faker.PickRandom(body, Math.Min(50, (int) (actual.field.Count * 0.10)))
                .ToArray();

            selection.Select(arg => arg.actualValue)
                .Should()
                .BeEquivalentTo(selection.Select(arg => arg.expectedValue));
        }
    }
}
