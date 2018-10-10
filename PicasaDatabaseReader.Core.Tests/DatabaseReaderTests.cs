using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Scheduling;
using PicasaDatabaseReader.Core.Tests.Extensions;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PicasaDatabaseReader.Core.Tests
{
    public class MatchStreamTests : UnitTestsBase<MatchStreamTests>
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();

        public MatchStreamTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public void ShouldSkipItems()
        {
            var source = new[] { 1, 2, 3 }.ToObservable();
            var remaining = SkipNextItems(source, new[] { 1, 2 });

            var autoResetEvent = new AutoResetEvent(false);

            int[] result = null;
            remaining.ToArray()
                .Subscribe(
                    values => result = values,
                    () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            result.Should().BeEquivalentTo(3);
        }

        [Fact]
        public void ShouldError()
        {
            var source = new[] { 1, 2, 3 }.ToObservable();
            var remainingItems = SkipNextItems(source, new[] { 1, 1 });

            var autoResetEvent = new AutoResetEvent(false);

            Exception remainingResult = null;
            remainingItems.ToArray()
                .Subscribe(Observer.Create<int[]>(ints => { }, onError: exception =>
                {
                    autoResetEvent.Set();
                }));

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        private IObservable<T> SkipNextItems<T>(IObservable<T> source, T[] items)
        {
            var count = 0;
            return source
                .Select(item => new { index = count++, item })
                .SkipWhile(arg => arg.index < items.Length && (arg.item.Equals(items[arg.index]) || ThrowException()))
                .Select(arg => arg.item);
        }

        [Fact(Timeout = 2000)]
        public async Task ShouldCaptureItems()
        {
            var source = new[] { 1, 2, 3, 4 }.ToObservable()
                .Do(i => Logger.LogInformation("Doing this {0}", i));

            

//            var items = CaptureNextItems(source, 2);
//
//            items.Subscribe(observable => )


        }

        private IObservable<IObservable<T>> CaptureNextItems<T>(IObservable<T> source, int size)
        {
            var count = 0;
            var closingSelector = new Subject<Unit>();

            return source
                .Do(observable =>
                {
                    if (count < size)
                    {
                        count++;
                    }
                    else if (count == size)
                    {
                        closingSelector.OnNext(Unit.Default);
                    }
                })
                .Window(windowClosingSelector: () => closingSelector);
        }

        private bool ThrowException()
        {
            throw new Exception();
        }
    }

    public class DatabaseReaderTests : UnitTestsBase<DatabaseReaderTests>
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();

        public DatabaseReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"{name}_0";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(BitConverter.GetBytes(DatabaseReader.TableFileHeader));
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);

            var tableNames = databaseReader
                .GetTableNames();

            var autoResetEvent = new AutoResetEvent(false);

            tableNames.Subscribe(_ => { }, () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact]
        public async Task ShouldGetFieldFiles()
        {
            Logger.LogInformation("ShouldGetFieldFiles");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"TestTable_{name}.pmp";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(string.Empty);
            var mockFiles = args.ToDictionary(arg => arg.path, _ => mockFileData);

            var mockFileSystem = new MockFileSystem(mockFiles);

            var databaseReader = this.CreateDatabaseReader(mockFileSystem, directoryPath, TestScheduleProvider);

            var tableNames = await databaseReader
                .GetFieldFilePaths("TestTable")
                .ToArray()
                .FirstAsync();

            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.path));
        }
    }
}
