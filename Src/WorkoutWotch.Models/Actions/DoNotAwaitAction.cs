namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Utility;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class DoNotAwaitAction : IAction
    {
        private readonly ILogger logger;
        private readonly IAction innerAction;

        public DoNotAwaitAction(ILoggerService loggerService, IAction innerAction)
        {
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(innerAction, nameof(innerAction));

            this.logger = loggerService.GetLogger(this.GetType());
            this.innerAction = innerAction;
        }

        public TimeSpan Duration => TimeSpan.Zero;

        public IAction InnerAction => this.innerAction;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            this
                .innerAction
                .Execute(context)
                .Subscribe(
                    _ => { },
                    ex => this.logger.Error("Failed to execute inner action: " + ex));

            return Observable.Return(Unit.Default);
        }
    }
}