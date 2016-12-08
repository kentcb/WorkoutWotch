namespace WorkoutWotch.UnitTests.Services.Delay.Mocks
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using PCLMock;

    public sealed partial class DelayServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Return(Observables.Unit);
        }
    }
}