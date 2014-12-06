using System;
using System.Threading.Tasks;
using WorkoutWotch.Services.Contracts.Audio;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Actions
{
    public sealed class AudioAction : IAction
    {
        private readonly IAudioService audioService;
        private readonly string audioResourceUri;

        public AudioAction(IAudioService audioService, string audioResourceUri)
        {
            audioService.AssertNotNull("audioService");
            audioResourceUri.AssertNotNull("audioResourceUri");

            this.audioService = audioService;
            this.audioResourceUri = audioResourceUri;
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

            await this.audioService
                .PlayAsync(this.audioResourceUri)
                .ContinueOnAnyContext();
        }
    }
}

