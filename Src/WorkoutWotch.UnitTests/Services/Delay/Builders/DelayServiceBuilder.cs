namespace WorkoutWotch.UnitTests.Services.Delay.Builders
{
    using PCLMock;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;

    public sealed class DelayServiceBuilder : IBuilder
    {
        private ISchedulerService schedulerService;

        public DelayServiceBuilder()
        {
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);
        }

        public DelayServiceBuilder WithSchedulerService(ISchedulerService schedulerService) =>
            this.With(ref this.schedulerService, schedulerService);

        public DelayService Build() =>
            new DelayService(
                this.schedulerService);

        public static implicit operator DelayService(DelayServiceBuilder builder) =>
            builder.Build();
    }
}