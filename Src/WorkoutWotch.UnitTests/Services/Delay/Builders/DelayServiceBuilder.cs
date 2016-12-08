namespace WorkoutWotch.UnitTests.Services.Delay.Builders
{
    using System.Reactive.Concurrency;
    using Genesis.TestUtil;
    using PCLMock;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;

    public sealed class DelayServiceBuilder : IBuilder
    {
        private IScheduler scheduler;

        public DelayServiceBuilder()
        {
            this.scheduler = new SchedulerMock(MockBehavior.Loose);
        }

        public DelayServiceBuilder WithScheduler(IScheduler scheduler) =>
            this.With(ref this.scheduler, scheduler);

        public DelayService Build() =>
            new DelayService(
                this.scheduler);

        public static implicit operator DelayService(DelayServiceBuilder builder) =>
            builder.Build();
    }
}