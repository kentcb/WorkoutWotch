namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class SayAction : IAction
    {
        private readonly ISpeechService speechService;
        private readonly string speechText;

        public SayAction(ISpeechService speechService, string speechText)
        {
            Ensure.ArgumentNotNull(speechService, nameof(speechService));
            Ensure.ArgumentNotNull(speechText, nameof(speechText));

            this.speechService = speechService;
            this.speechText = speechText;
        }

        public TimeSpan Duration => TimeSpan.Zero;

        public string SpeechText => this.speechText;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            return context
                .WaitWhilePaused()
                .SelectMany(_ => this.speechService.Speak(this.speechText))
                .FirstAsync();
        }
    }
}