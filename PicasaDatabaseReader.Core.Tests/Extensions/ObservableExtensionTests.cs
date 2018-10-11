using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using PicasaDatabaseReader.Core.Extensions;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests.Extensions
{
    public class ObservableExtensionTests : UnitTestsBase<ObservableExtensionTests>
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();

        public ObservableExtensionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public void ShouldMatchItems()
        {
            var source = new[] { 1, 2, 3 }.ToObservable();
            var remaining = source.MatchNextItems(new[] { 1, 2 });

            var autoResetEvent = new AutoResetEvent(false);

            int[] result = null;
            remaining.ToArray()
                .Subscribe(
                    values => result = values,
                    onCompleted: () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            result.Should().BeEquivalentTo(3);
        }

        [Fact]
        public void ShouldThrowErrorIfNotMatch()
        {
            var source = new[] { 1, 2, 3 }.ToObservable();
            var remainingItems = source.MatchNextItems(new[] { 1, 1 });

            var autoResetEvent = new AutoResetEvent(false);

            Exception remainingResult = null;
            remainingItems.ToArray()
                .Subscribe(Observer.Create<int[]>(
                    _ => { },
                    onError: _ => autoResetEvent.Set()));

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact(Timeout = 1000)]
        public void ShouldThrowErrorSourceEndsEarly()
        {
            var source = new[] { 1, 2, 3 }.ToObservable();
            var remainingItems = source.MatchNextItems(new[] { 1, 2, 3, 4 });

            var autoResetEvent = new AutoResetEvent(false);

            Exception remainingResult = null;
            remainingItems.ToArray()
                .Subscribe(Observer.Create<int[]>(
                    _ => { },
                    onError: _ => autoResetEvent.Set()));

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact]
        public void ShouldCaptureItems()
        {
            var source = new[] { 1, 2, 3 }.ToObservable();

            var autoResetEvent = new AutoResetEvent(false);

            int[] result = null;
            source
                .CaptureNextItems(3, ints => result = ints)
                .Subscribe(
                    _ => { },
                    onCompleted: () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            result.Length.Should().Be(3);
            result.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void ShouldSkipAndCaptureItems()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 }.ToObservable();

            var autoResetEvent = new AutoResetEvent(false);

            int[] result = null;
            int[] remaining = null;

            source
                .Skip(1)
                .CaptureNextItems(3, ints => result = ints)
                .Skip(1)
                .ToArray()
                .Subscribe(
                    ints => remaining = ints,
                    onCompleted: () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            result.Length.Should().Be(3);
            result.Should().BeEquivalentTo(2, 3, 4);
            remaining.Should().BeEquivalentTo(6);
        }

        [Fact]
        public void ShouldMatchAndCaptureItems()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 }.ToObservable();

            var autoResetEvent = new AutoResetEvent(false);

            int[] result = null;
            int[] remaining = null;

            source
                .MatchNextItems(1)
                .CaptureNextItems(3, ints => result = ints)
                .MatchNextItems(5)
                .ToArray()
                .Subscribe(
                    ints => remaining = ints,
                    onCompleted: () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            result.Length.Should().Be(3);
            result.Should().BeEquivalentTo(2, 3, 4);
            remaining.Should().BeEquivalentTo(6);
        }
    }
}