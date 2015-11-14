namespace WorkoutWotch.UnitTests.Services.Delay.Builders
{
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;

    public sealed class DelayServiceBuilder
    {
        private ISchedulerService schedulerService;

        public DelayServiceBuilder()
        {
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);
        }

        public DelayServiceBuilder WithSchedulerService(ISchedulerService schedulerService)
        {
            this.schedulerService = schedulerService;
            return this;
        }

        public DelayService Build() =>
            new DelayService(
                this.schedulerService);

        public static implicit operator DelayService(DelayServiceBuilder builder) =>
            builder.Build();
    }
}