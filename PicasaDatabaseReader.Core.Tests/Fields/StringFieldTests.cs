using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PicasaDatabaseReader.Core.Fields;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests
{
    public class StringFieldTests : UnitTestsBase<StringFieldTests>
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();

        public StringFieldTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        [Fact]
        public async Task blah()
        {
            var wordsArray = Faker.Random.WordsArray(3);

            var data = new byte[20]
                .Concat(wordsArray
                    .Select(Encoding.UTF8.GetBytes)
                    .Select(bytes1 => bytes1.Append((byte)0x0).ToArray())
                    .SelectMany(bytes1 => bytes1))
                .ToArray();

            var mockFileSystem = new MockFileSystem();

            mockFileSystem.AddFile("c:\\input.pmp", new MockFileData(data));

            var stringField = new StringField("Test", "c:\\input.pmp", 3, mockFileSystem);
            var result = await stringField.GetValues().Cast<string>().ToArray().FirstAsync();

            result.Should().BeEquivalentTo(wordsArray);
        }
    }
}
