namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Utility;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioAction : IAction
    {
        private readonly IAudioService audioService;
        private readonly string audioName;

        public AudioAction(IAudioService audioService, string audioName)
        {
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(audioName, nameof(audioName));

            this.audioService = audioService;
            this.audioName = audioName;
        }

        public TimeSpan Duration => TimeSpan.Zero;

        public IObservable<Unit> ExecuteAsync(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            return context
                .WaitWhilePausedAsync()
                .SelectMany(_ => this.audioService.PlayAsync(this.audioName))
                .FirstAsync()
                .RunAsync(context.CancellationToken);
        }
    }
}