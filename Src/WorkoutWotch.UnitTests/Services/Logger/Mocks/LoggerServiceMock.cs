namespace WorkoutWotch.UnitTests.Services.Logger.Mocks
{
    using System;
    using System.Reactive.Linq;
    using PCLMock;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed partial class LoggerServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.GetLogger(It.IsAny<Type>()))
                .Return(new LoggerMock(MockBehavior.Loose));
            this
                .When(x => x.GetLogger(It.IsAny<string>()))
                .Return(new LoggerMock(MockBehavior.Loose));
            this
                .When(x => x.Entries).Return(Observable.Empty<LogEntry>());
        }
    }
}