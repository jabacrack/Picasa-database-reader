using System.Linq;
using FluentAssertions;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.IntegrationTests.Legacy
{
    public class DbUtilsTests: TestsBase<DbUtilsTests>
    {
        public const string PathToDatabase = "C:\\Users\\Spade\\AppData\\Local\\Google\\Picasa2\\db3";

        public DbUtilsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ShouldGetTableNames()
        {
            var tableNames = DbUtils.GetTablesNames(PathToDatabase).ToArray();
            tableNames.Should().HaveCount(3);
        }

        [Theory]
        [InlineData("albumdata", 20)]
        [InlineData("catdata", 3)]
        [InlineData("imagedata", 36)]
        public void ShouldGetFieldsFiles(string tableName, int fieldCount)
        {
            var fieldsFiles = DbUtils.GetFieldsFiles(PathToDatabase, tableName).ToArray();
            fieldsFiles.Should().HaveCount(fieldCount);
        }
    }
}