namespace WorkoutWotch.UnitTests.Reactive
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using Microsoft.Reactive.Testing;

    public sealed class TestSchedulerService : TestScheduler, ISchedulerService
    {
        public IScheduler DefaultScheduler
        {
            get { return this; }
        }

        public IScheduler CurrentThreadScheduler
        {
            get { return this; }
        }

        public IScheduler ImmediateScheduler
        {
            get { return this; }
        }

        public IScheduler SynchronizationContextScheduler
        {
            get { return this; }
        }

        public IScheduler TaskPoolScheduler
        {
            get { return this; }
        }

        public IDisposable Pump()
        {
            return Pump(TimeSpan.FromMilliseconds(10));
        }

        // useful hack to allow tests to automatically pump any scheduled items
        public IDisposable Pump(TimeSpan frequency)
        {
            return Observable.Timer(TimeSpan.Zero, frequency).Subscribe(_ => this.Start());
        }
    }
}