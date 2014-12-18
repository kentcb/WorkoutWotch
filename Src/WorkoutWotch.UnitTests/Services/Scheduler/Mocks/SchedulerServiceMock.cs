namespace WorkoutWotch.UnitTests.Services.Scheduler.Mocks
{
    using System.Reactive.Concurrency;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Scheduler;

    public sealed class SchedulerServiceMock : MockBase<ISchedulerService>, ISchedulerService
    {
        public SchedulerServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.DefaultScheduler).Return(new SchedulerMock(MockBehavior.Loose));
                this.When(x => x.CurrentThreadScheduler).Return(new SchedulerMock(MockBehavior.Loose));
                this.When(x => x.ImmediateScheduler).Return(new SchedulerMock(MockBehavior.Loose));
                this.When(x => x.SynchronizationContextScheduler).Return(new SchedulerMock(MockBehavior.Loose));
                this.When(x => x.TaskPoolScheduler).Return(new SchedulerMock(MockBehavior.Loose));
            }
        }

        public IScheduler DefaultScheduler
        {
            get { return this.Apply(x => x.DefaultScheduler); }
        }

        public IScheduler CurrentThreadScheduler
        {
            get { return this.Apply(x => x.CurrentThreadScheduler); }
        }

        public IScheduler ImmediateScheduler
        {
            get { return this.Apply(x => x.ImmediateScheduler); }
        }

        public IScheduler SynchronizationContextScheduler
        {
            get { return this.Apply(x => x.SynchronizationContextScheduler); }
        }

        public IScheduler TaskPoolScheduler
        {
            get { return this.Apply(x => x.TaskPoolScheduler); }
        }
    }
}