namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System;
    using PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;

    public sealed class WaitActionBuilder : IBuilder
    {
        private IDelayService delayService;
        private TimeSpan delay;

        public WaitActionBuilder()
        {
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
        }

        public WaitActionBuilder WithDelayService(IDelayService delayService) =>
            this.With(ref this.delayService, delayService);

        public WaitActionBuilder WithDelay(TimeSpan delay) =>
            this.With(ref this.delay, delay);

        public WaitAction Build() =>
            new WaitAction(
                this.delayService,
                this.delay);

        public static implicit operator WaitAction(WaitActionBuilder builder) =>
            builder.Build();
    }
}