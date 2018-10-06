using System.Reactive.Concurrency;

namespace PicasaDatabaseReader.Core.Scheduling
{
    public interface ISchedulerProvider
    {
        IScheduler CurrentThread { get; }
        IScheduler Immediate { get; }
        IScheduler NewThread { get; }
        IScheduler ThreadPool { get; }
        IScheduler TaskPool { get; }
    }
}