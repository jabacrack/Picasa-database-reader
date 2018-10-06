using Bogus;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public abstract class UnitTestsBase<T> : TestsBase<T>
    {
        protected readonly Faker Faker;

        protected UnitTestsBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Faker = new Faker();
        }
    }
}