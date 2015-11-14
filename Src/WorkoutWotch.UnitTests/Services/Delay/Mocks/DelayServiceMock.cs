namespace WorkoutWotch.UnitTests.Services.Delay.Mocks
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Kent.Boogaart.PCLMock;

    public sealed partial class DelayServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Return(Observable.Return(Unit.Default));
        }
    }
}