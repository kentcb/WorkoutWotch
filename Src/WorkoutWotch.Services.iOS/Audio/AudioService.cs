namespace WorkoutWotch.Services.iOS.Audio
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using AVFoundation;
    using Contracts.Scheduler;
    using Foundation;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Utility;

    public sealed class AudioService : IAudioService
    {
        private readonly ISchedulerService schedulerService;

        public AudioService(ISchedulerService schedulerService)
        {
            Ensure.ArgumentNotNull(schedulerService, nameof(schedulerService));

            this.schedulerService = schedulerService;
        }

        public IObservable<Unit> PlayAsync(string name)
        {
            Ensure.ArgumentNotNull(name, nameof(name));

            var url = new NSUrl("Audio/" + name + ".mp3");
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