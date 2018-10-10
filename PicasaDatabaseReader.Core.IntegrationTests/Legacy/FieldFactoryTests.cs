using System.Linq;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.IntegrationTests.Legacy
{
    public class FieldFactoryTests : TestsBase<DbUtilsTests>
    {
        public FieldFactoryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void AlbumDataTableFields()
        {
            var fields = DbUtils.GetFieldsFiles(DbUtilsTests.PathToDatabase, "albumdata")
                .Select(PicasaDatabaseReader.FieldFactory.CreateField)
                .ToArray();
        }

        [Fact]
        public void CatDataTableFields()
        {
            var fields = DbUtils.GetFieldsFiles(DbUtilsTests.PathToDatabase, "catdata")
                .Select(PicasaDatabaseReader.FieldFactory.CreateField)
                .ToArray();
        }

        [Fact]
        public void ImageDataTableFields()
        {
            var fields = DbUtils.GetFieldsFiles(DbUtilsTests.PathToDatabase, "imagedata")
                .Select(PicasaDatabaseReader.FieldFactory.CreateField)
                .ToArray();
        }
    }
}