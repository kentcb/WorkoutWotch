namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    internal sealed class DoNotAwaitActionBuilder
    {
        private ILoggerService loggerService;
        private IAction innerAction;

        public DoNotAwaitActionBuilder()
        {
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.innerAction = new ActionMock(MockBehavior.Loose);
        }

        public DoNotAwaitActionBuilder WithLoggerService(ILoggerService loggerService)
        {
            this.loggerService = loggerService;
            return this;
        }

        public DoNotAwaitActionBuilder WithInnerAction(IAction innerAction)
        {
            this.innerAction = innerAction;
            return this;
        }

        public DoNotAwaitAction Build() =>
            new DoNotAwaitAction(
                this.loggerService,
                this.innerAction);

        public static implicit operator DoNotAwaitAction(DoNotAwaitActionBuilder builder) =>
            builder.Build();
    }
}