using System;
using System.Reactive.Concurrency;

namespace WorkoutWotch.Services.Contracts.Scheduler
{
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

