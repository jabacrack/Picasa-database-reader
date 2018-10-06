using System.Linq;
using FluentAssertions;
using PicasaDatabaseReader.Core.Tests;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.IntegrationTests
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

    public class FieldFactoryTests : TestsBase<DbUtilsTests>
    {
        public FieldFactoryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void AlbumDataTableFields()
        {
            var fields = DbUtils.GetFieldsFiles(DbUtilsTests.PathToDatabase, "albumdata")
                .Select(FieldFactory.CreateField)
                .ToArray();
        }

        [Fact]
        public void CatDataTableFields()
        {
            var fields = DbUtils.GetFieldsFiles(DbUtilsTests.PathToDatabase, "catdata")
                .Select(FieldFactory.CreateField)
                .ToArray();
        }

        [Fact]
        public void ImageDataTableFields()
        {
            var fields = DbUtils.GetFieldsFiles(DbUtilsTests.PathToDatabase, "imagedata")
                .Select(FieldFactory.CreateField)
                .ToArray();
        }
    }
}