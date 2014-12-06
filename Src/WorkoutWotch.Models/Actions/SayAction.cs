using System;
using System.Threading.Tasks;
using WorkoutWotch.Services.Contracts.Speech;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Actions
{
    public sealed class SayAction : IAction
    {
        private readonly ISpeechService speechService;
        private readonly string speechText;

        public SayAction(ISpeechService speechService, string speechText)
        {
            speechService.AssertNotNull("speechService");
            speechText.AssertNotNull("speechText");

            this.speechService = speechService;
            this.speechText = speechText;
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.Zero; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");
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

