namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioAction : IAction
    {
        private readonly IAudioService audioService;
        private readonly string audioName;

        public AudioAction(IAudioService audioService, string audioName)
        {
            audioService.AssertNotNull(nameof(audioService));
            audioName.AssertNotNull(nameof(audioName));

            this.audioService = audioService;
            this.audioName = audioName;
        }

        public TimeSpan Duration => TimeSpan.Zero;

        public IObservable<Unit> ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            return context
                .WaitWhilePausedAsync()
                .SelectMany(_ => this.audioService.PlayAsync(this.audioName))
                .FirstAsync()
                .RunAsync(context.CancellationToken);
        }
    }
}