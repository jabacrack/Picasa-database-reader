using System.Linq;
using FluentAssertions;
using Xunit;

namespace PicasaDatabaseReader.Core.IntegrationTests
{
    public class DbUtilsTests
    {
        protected internal const string PathToDatabase = "C:\\Users\\Spade\\AppData\\Local\\Google\\Picasa2\\db3";

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