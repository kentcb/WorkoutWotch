namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioAction : IAction
    {
        private readonly IAudioService audioService;
        private readonly string audioResourceUri;

        public AudioAction(IAudioService audioService, string audioResourceUri)
        {
            audioService.AssertNotNull(nameof(audioService));
            audioResourceUri.AssertNotNull(nameof(audioResourceUri));

            this.audioService = audioService;
            this.audioResourceUri = audioResourceUri;
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.Zero; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));
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