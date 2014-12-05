namespace WorkoutWotch.UnitTests.Services.Delay.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class DelayServiceMock : MockBase<IDelayService>, IDelayService
    {
        public DelayServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Return(Task.FromResult(true));
            }
        }

        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.Apply(x => x.DelayAsync(duration, cancellationToken));
        }
    }
}