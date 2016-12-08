namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    public sealed class DoNotAwaitActionBuilder : IBuilder
    {
        private ILoggerService loggerService;
        private IAction innerAction;

        public DoNotAwaitActionBuilder()
        {
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.innerAction = new ActionMock(MockBehavior.Loose);
        }

        public DoNotAwaitActionBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public DoNotAwaitActionBuilder WithInnerAction(IAction innerAction) =>
            this.With(ref this.innerAction, innerAction);

        public DoNotAwaitAction Build() =>
            new DoNotAwaitAction(
                this.loggerService,
                this.innerAction);

        public static implicit operator DoNotAwaitAction(DoNotAwaitActionBuilder builder) =>
            builder.Build();
    }
}