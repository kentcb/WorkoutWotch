namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;

    internal sealed class WaitActionBuilder
    {
        private IDelayService delayService;
        private TimeSpan delay;

        public WaitActionBuilder()
        {
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
        }

        public WaitActionBuilder WithDelayService(IDelayService delayService)
        {
            this.delayService = delayService;
            return this;
        }

        public WaitActionBuilder WithDelay(TimeSpan delay)
        {
            this.delay = delay;
            return this;
        }

        public WaitAction Build() =>
            new WaitAction(
                this.delayService,
                this.delay);

        public static implicit operator WaitAction(WaitActionBuilder builder) =>
            builder.Build();
    }
}