namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class BreakAction : IAction
    {
        private readonly WaitWithPromptAction innerAction;

        public BreakAction(IDelayService delayService, ISpeechService speechService, TimeSpan duration)
        {
            this.innerAction = new WaitWithPromptAction(delayService, speechService, duration, "break");
        }

        public TimeSpan Duration => this.innerAction.Duration;

        public IObservable<Unit> Execute(ExecutionContext context) =>
            this
                .innerAction
                .Execute(context);
    }
}