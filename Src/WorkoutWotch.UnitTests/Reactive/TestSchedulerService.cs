namespace WorkoutWotch.UnitTests.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Microsoft.Reactive.Testing;
    using WorkoutWotch.Services.Contracts.Scheduler;

    public sealed class TestSchedulerService : ISchedulerService
    {
        private readonly bool hasSingleUnderlyingScheduler;
        private readonly TestScheduler defaultScheduler;
        private readonly TestScheduler currentThreadScheduler;
        private readonly TestScheduler immediateScheduler;
        private readonly TestScheduler synchronizationContextScheduler;
        private readonly TestScheduler eventLoopScheduler;
        private readonly TestScheduler newThreadScheduler;
        private readonly TestScheduler taskPoolScheduler;

        public TestSchedulerService(bool singleUnderlyingScheduler = true)
        {
            this.hasSingleUnderlyingScheduler = singleUnderlyingScheduler;
            this.defaultScheduler = new TestScheduler();

            if (singleUnderlyingScheduler)
            {
                this.currentThreadScheduler =
                    this.immediateScheduler =
                    this.synchronizationContextScheduler =
                    this.eventLoopScheduler =
                    this.newThreadScheduler =
                    this.taskPoolScheduler =
                    this.defaultScheduler;
            }
            else
            {
                this.currentThreadScheduler = new TestScheduler();
                this.immediateScheduler = new TestScheduler();
                this.synchronizationContextScheduler = new TestScheduler();
                this.eventLoopScheduler = new TestScheduler();
                this.newThreadScheduler = new TestScheduler();
                this.taskPoolScheduler = new TestScheduler();
            }
        }

        public IScheduler DefaultScheduler => this.defaultScheduler;

        public IScheduler CurrentThreadScheduler => this.currentThreadScheduler;

        public IScheduler ImmediateScheduler => this.immediateScheduler;

        public IScheduler SynchronizationContextScheduler => this.synchronizationContextScheduler;

        public IScheduler EventLoopScheduler => this.eventLoopScheduler;

        public IScheduler NewThreadScheduler => this.newThreadScheduler;

        public IScheduler TaskPoolScheduler => this.taskPoolScheduler;

        public IDisposable Pump() =>
            Pump(TimeSpan.FromMilliseconds(10));

        // useful hack to allow tests to automatically pump any scheduled items
        public IDisposable Pump(TimeSpan frequency) =>
            new CompositeDisposable(
                Observable
                    .Timer(TimeSpan.Zero, frequency)
                    .Subscribe(_ => this.AdvanceMinimal()),
                Disposable.Create(() => this.Stop()));

        public void Start()
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.Start();
            }
        }

        public void AdvanceBy(long time)
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.AdvanceBy(time);
            }
        }

        public void AdvanceTo(long time)
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.AdvanceTo(time);
            }
        }

        public void Stop()
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.Stop();
            }
        }

        private IEnumerable<TestScheduler> GetTestSchedulers()
        {
            yield return this.defaultScheduler;

            if (this.hasSingleUnderlyingScheduler)
            {
                yield break;
            }

            yield return this.defaultScheduler;
            yield return this.currentThreadScheduler;
            yield return this.immediateScheduler;
            yield return this.synchronizationContextScheduler;
            yield return this.eventLoopScheduler;
            yield return this.newThreadScheduler;
            yield return this.taskPoolScheduler;
        }
    }
}