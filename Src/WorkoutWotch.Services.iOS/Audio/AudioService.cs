namespace WorkoutWotch.Services.iOS.Audio
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using AVFoundation;
    using Contracts.Scheduler;
    using Foundation;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioService : IAudioService
    {
        private readonly ISchedulerService schedulerService;

        public AudioService(ISchedulerService schedulerService)
        {
            schedulerService.AssertNotNull(nameof(schedulerService));

            this.schedulerService = schedulerService;
        }

        public IObservable<Unit> PlayAsync(string resourceUri)
        {
            resourceUri.AssertNotNull(nameof(resourceUri));

            var url = new NSUrl(resourceUri);
            var audioPlayer = AVAudioPlayer.FromUrl(url);
            var finishedPlaying = Observable
                .FromEventPattern<AVStatusEventArgs>(x => audioPlayer.FinishedPlaying += x, x => audioPlayer.FinishedPlaying -= x)
                .Select(_ => Unit.Default)
                .Publish();

            finishedPlaying
                .ObserveOn(this.schedulerService.MainScheduler)
                .Subscribe(_ => audioPlayer.Dispose());

            finishedPlaying.Connect();
            audioPlayer.Play();

            return finishedPlaying
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }
    }
}