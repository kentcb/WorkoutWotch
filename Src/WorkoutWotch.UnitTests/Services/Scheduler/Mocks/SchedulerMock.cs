namespace WorkoutWotch.UnitTests.Services.Scheduler.Mocks
{
    using System;
    using System.Reactive.Concurrency;
    using PCLMock;

    public sealed class SchedulerMock : MockBase<IScheduler>, IScheduler
    {
        public SchedulerMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.Now).Return(() => DateTimeOffset.UtcNow);
            }
        }

        public DateTimeOffset Now
        {
            get { return this.Apply(x => x.Now); }
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return this.Apply(x => x.Schedule<TState>(state, action));
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return this.Apply(x => x.Schedule<TState>(state, dueTime, action));
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return this.Apply(x => x.Schedule<TState>(state, dueTime, action));
        }
    }
}