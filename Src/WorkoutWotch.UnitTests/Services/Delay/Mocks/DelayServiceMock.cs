namespace WorkoutWotch.UnitTests.Services.Delay.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;

    public sealed partial class DelayServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Return(Task.FromResult(true));
        }
    }
}