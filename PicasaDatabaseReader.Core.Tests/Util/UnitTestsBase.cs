using Bogus;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public abstract class UnitTestsBase<T> : TestsBase<T>
    {
        protected UnitTestsBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}