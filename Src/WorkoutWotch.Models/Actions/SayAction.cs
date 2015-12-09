namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class SayAction : IAction
    {
        private readonly ISpeechService speechService;
        private readonly string speechText;

        public SayAction(ISpeechService speechService, string speechText)
        {
            speechService.AssertNotNull(nameof(speechService));
            speechText.AssertNotNull(nameof(speechText));

            this.speechService = speechService;
            this.speechText = speechText;
        }

        public TimeSpan Duration => TimeSpan.Zero;

        public string SpeechText => this.speechText;

        public IObservable<Unit> ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            return context
                .WaitWhilePausedAsync()
                .SelectMany(_ => this.speechService.SpeakAsync(this.speechText, context.CancellationToken))
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }
    }
}