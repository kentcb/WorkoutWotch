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

        IScheduler MainScheduler
        {
            get;
        }

        IScheduler TaskPoolScheduler
        {
            get;
        }
    }
}