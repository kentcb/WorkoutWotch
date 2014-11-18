using System;
using WorkoutWotch.Services.Contracts.Scheduler;
using System.Reactive.Concurrency;
using System.Threading;

namespace WorkoutWotch.Services.Scheduler
{
    public sealed class SchedulerService : ISchedulerService
    {
        private readonly IScheduler synchronizationContextScheduler;

        public SchedulerService()
        {
            this.synchronizationContextScheduler = new System.Reactive.Concurrency.SynchronizationContextScheduler(SynchronizationContext.Current);
        }

        #region ISchedulerService implementation

        public IScheduler DefaultScheduler
        {
            get
            {
                return System.Reactive.Concurrency.DefaultScheduler.Instance;
            }
        }

        public IScheduler CurrentThreadScheduler
        {
            get
            {
                return System.Reactive.Concurrency.CurrentThreadScheduler.Instance;
            }
        }

        public IScheduler ImmediateScheduler
        {
            get
            {
                return System.Reactive.Concurrency.ImmediateScheduler.Instance;
            }
        }

        public IScheduler SynchronizationContextScheduler
        {
            get
            {
                return this.synchronizationContextScheduler;
            }
        }

        public IScheduler TaskPoolScheduler
        {
            get
            {
                return System.Reactive.Concurrency.TaskPoolScheduler.Default;
            }
        }

        #endregion
    }
}

