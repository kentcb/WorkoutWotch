namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using Genesis.Logging;

    public sealed class DoNotAwaitAction : IAction
    {
        private readonly ILogger logger;
        private readonly IAction innerAction;

        public DoNotAwaitAction(IAction innerAction)
        {
            Ensure.ArgumentNotNull(innerAction, nameof(innerAction));

            this.logger = LoggerService.GetLogger(this.GetType());
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
                .SubscribeSafe();

            return Observables.Unit;
        }
    }
}