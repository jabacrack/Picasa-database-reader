using System;
using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace PicasaDatabaseReader.Core.IntegrationTests
{
    public class DatabaseReaderTests
    {
        protected internal const string PathToDatabase = "C:\\Users\\Spade\\AppData\\Local\\Google\\Picasa2\\db3";

        [Fact]
        public async Task ShouldReadTables()
        {
            var fileSystem = new FileSystem();
            var databaseReader = new DatabaseReader(fileSystem, PathToDatabase);

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
        public async Task ShouldGetFieldsFiles(string tableName, int fieldCount)
        {
            var fileSystem = new FileSystem();
            var databaseReader = new DatabaseReader(fileSystem, PathToDatabase);

            var fields = await databaseReader
                .GetFieldFilePaths(tableName)
                .ToArray()
                .FirstAsync();

            fields.Should().HaveCount(fieldCount);
        }
    }
}
