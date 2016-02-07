namespace WorkoutWotch.Services.iOS.Audio
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using AVFoundation;
    using Foundation;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Utility;

    public sealed class AudioService : IAudioService
    {
        private readonly IScheduler scheduler;

        public AudioService(IScheduler scheduler)
        {
            Ensure.ArgumentNotNull(scheduler, nameof(scheduler));

            this.scheduler = scheduler;
        }

        public IObservable<Unit> Play(string name)
        {
            Ensure.ArgumentNotNull(name, nameof(name));

            var url = new NSUrl("Audio/" + name + ".mp3");
            var audioPlayer = AVAudioPlayer.FromUrl(url);
            var finishedPlaying = Observable
                .FromEventPattern<AVStatusEventArgs>(x => audioPlayer.FinishedPlaying += x, x => audioPlayer.FinishedPlaying -= x)
                .Select(_ => Unit.Default)
                .Publish();

            finishedPlaying
                .ObserveOn(this.scheduler)
                .Subscribe(_ => audioPlayer.Dispose());

            finishedPlaying.Connect();
            audioPlayer.Play();

            return finishedPlaying
                .FirstAsync();
        }
    }
}