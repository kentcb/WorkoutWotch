namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using Kent.Boogaart.PCLMock;
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

        public DoNotAwaitAction Build()
        {
            return new DoNotAwaitAction(
                this.loggerService,
                this.innerAction);
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
    }
}