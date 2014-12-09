namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class DoNotAwaitAction : IAction
    {
        private readonly ILogger logger;
        private readonly IAction innerAction;

        public DoNotAwaitAction(ILoggerService loggerService, IAction innerAction)
        {
            loggerService.AssertNotNull("loggerService");
            innerAction.AssertNotNull("innerAction");

            this.logger = loggerService.GetLogger(this.GetType());
            this.innerAction = innerAction;
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.Zero; }
        }

        public Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");

            this.innerAction
                .ExecuteAsync(context)
                .ContinueWith(
                    x =>
                    {
                        if (x.IsFaulted)
                        {
                            this.logger.Error("Failed to execute inner action: " + x.Exception);
                        }
                    });

            return Task.FromResult(true);
        }
    }
}