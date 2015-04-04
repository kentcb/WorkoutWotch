namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Threading.Tasks;
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

        public TimeSpan Duration
        {
            get { return TimeSpan.Zero; }
        }

        public string SpeechText
        {
            get { return this.speechText; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            await context
                .WaitWhilePausedAsync()
                .ContinueOnAnyContext();

            await this.speechService
                .SpeakAsync(this.speechText, context.CancellationToken)
                .ContinueOnAnyContext();
        }
    }
}