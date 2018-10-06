using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using PicasaDatabaseReader.Core.Scheduling;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public sealed class TestScheduleProvider : ISchedulerProvider
    {
        public TestScheduler CurrentThread { get; } = new TestScheduler();
        public TestScheduler Immediate { get; } = new TestScheduler();
        public TestScheduler NewThread { get; } = new TestScheduler();
        public TestScheduler ThreadPool { get; } = new TestScheduler();
        public TestScheduler TaskPool { get; } = new TestScheduler();

        IScheduler ISchedulerProvider.CurrentThread => CurrentThread;
        IScheduler ISchedulerProvider.Immediate => Immediate;
        IScheduler ISchedulerProvider.NewThread => NewThread;
        IScheduler ISchedulerProvider.ThreadPool => ThreadPool;
        IScheduler ISchedulerProvider.TaskPool => TaskPool;
    }
}