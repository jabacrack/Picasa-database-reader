using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Tests;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.IntegrationTests
{
    public class DatabaseReaderTests : TestsBase<DatabaseReaderTests>
    {

        public DatabaseReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected internal const string PathToDatabase = "C:\\Users\\Spade\\AppData\\Local\\Google\\Picasa2\\db3";

        [Fact]
        public async Task ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var fileSystem = new FileSystem();
            var databaseReader = new DatabaseReader(fileSystem, PathToDatabase, GetLogger<DatabaseReader>());

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
            var databaseReader = new DatabaseReader(fileSystem, PathToDatabase, GetLogger<DatabaseReader>());

            var fields = await databaseReader
                .GetFieldFilePaths(tableName)
                .ToArray()
                .FirstAsync();

            fields.Should().HaveCount(fieldCount);
        }
    }
}
