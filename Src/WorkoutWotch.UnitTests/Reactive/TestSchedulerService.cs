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
        private readonly TestScheduler mainScheduler;
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
                    this.mainScheduler =
                    this.eventLoopScheduler =
                    this.newThreadScheduler =
                    this.taskPoolScheduler =
                    this.defaultScheduler;
            }
            else
            {
                this.currentThreadScheduler = new TestScheduler();
                this.immediateScheduler = new TestScheduler();
                this.mainScheduler = new TestScheduler();
                this.eventLoopScheduler = new TestScheduler();
                this.newThreadScheduler = new TestScheduler();
                this.taskPoolScheduler = new TestScheduler();
            }
        }

        public IScheduler DefaultScheduler => this.defaultScheduler;

        public IScheduler CurrentThreadScheduler => this.currentThreadScheduler;

        public IScheduler ImmediateScheduler => this.immediateScheduler;

        public IScheduler MainScheduler => this.mainScheduler;

        public IScheduler EventLoopScheduler => this.eventLoopScheduler;

        public IScheduler NewThreadScheduler => this.newThreadScheduler;

        public IScheduler TaskPoolScheduler => this.taskPoolScheduler;

        public void AdvanceUntilEmpty()
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.AdvanceUntilEmpty();
            }
        }

        public void AdvanceBy(long ticks)
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.AdvanceBy(ticks);
            }
        }

        public void AdvanceBy(TimeSpan time) =>
            this.AdvanceBy(time.Ticks);

        public void AdvanceTo(long ticks)
        {
            foreach (var testScheduler in this.GetTestSchedulers())
            {
                testScheduler.AdvanceTo(ticks);
            }
        }

        public void AdvanceTo(DateTime dateTime) =>
            this.AdvanceTo(dateTime.Ticks);

        public void AdvanceMinimal() =>
            // not technically minimal, but advancing by only one or a few ticks doesn't always work as expected (a bug in Rx?)
            this.AdvanceBy(10);

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
            yield return this.mainScheduler;
            yield return this.eventLoopScheduler;
            yield return this.newThreadScheduler;
            yield return this.taskPoolScheduler;
        }
    }
}