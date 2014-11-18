namespace WorkoutWotch.Services.Contracts.Scheduler
{
    using System.Reactive.Concurrency;

    public interface ISchedulerService
    {
        IScheduler DefaultScheduler
        {
            get;
        }

        IScheduler CurrentThreadScheduler
        {
            get;
        }

        IScheduler ImmediateScheduler
        {
            get;
        }

        IScheduler SynchronizationContextScheduler
        {
            get;
        }

        IScheduler TaskPoolScheduler
        {
            get;
        }
    }
}