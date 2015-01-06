namespace WorkoutWotch.UnitTests.Models.Actions
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

        public WaitAction Build()
        {
            return new WaitAction(
                this.delayService,
                this.delay);
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

        public static implicit operator WaitAction(WaitActionBuilder builder)
        {
            return builder.Build();
        }
    }
}