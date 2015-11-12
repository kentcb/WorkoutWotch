namespace WorkoutWotch.UnitTests.Services.Logger.Mocks
{
    using System.Reactive.Disposables;
    using Kent.Boogaart.PCLMock;

    public sealed partial class LoggerMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Perf(It.IsAny<string>()))
                .Return(Disposable.Empty);
            this
                .When(x => x.Perf(It.IsAny<string>(), It.IsAny<object[]>()))
                .Return(Disposable.Empty);
        }
    }
}