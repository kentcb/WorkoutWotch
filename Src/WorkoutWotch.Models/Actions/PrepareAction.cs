namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Threading.Tasks;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class PrepareAction : IAction
    {
        private readonly WaitWithPromptAction innerAction;

        public PrepareAction(IDelayService delayService, ISpeechService speechService, TimeSpan duration)
        {
            this.innerAction = new WaitWithPromptAction(delayService, speechService, duration, "prepare");
        }

        public TimeSpan Duration => this.innerAction.Duration;

        public async Task ExecuteAsync(ExecutionContext context) =>
            await this
                .innerAction
                .ExecuteAsync(context)
                .ContinueOnAnyContext();
    }
}