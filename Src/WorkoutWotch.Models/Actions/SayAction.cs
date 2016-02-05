namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Utility;
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

        public IObservable<Unit> ExecuteAsync(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            return context
                .WaitWhilePausedAsync()
                .SelectMany(_ => this.speechService.SpeakAsync(this.speechText, context.CancellationToken))
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }
    }
}