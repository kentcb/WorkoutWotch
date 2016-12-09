namespace WorkoutWotch.Services.iOS.Audio
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using AVFoundation;
    using Foundation;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Audio;

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

            return Observable
                .Create<Unit>(
                    observer =>
                    {
                        var disposables = new CompositeDisposable();
                        var url = NSBundle.MainBundle.GetUrlForResource(name, "mp3", "Audio");
                        var audioPlayer = AVAudioPlayer.FromUrl(url);
                        var finishedPlaying = Observable
                            .FromEventPattern<AVStatusEventArgs>(x => audioPlayer.FinishedPlaying += x, x => audioPlayer.FinishedPlaying -= x)
                            .FirstAsync()
                            .Select(_ => Unit.Default)
                            .Publish();

                        finishedPlaying
                            .Subscribe(observer)
                            .AddTo(disposables);

                        finishedPlaying
                            .ObserveOn(this.scheduler)
                            .Subscribe(_ => audioPlayer.Dispose())
                            .AddTo(disposables);

                        finishedPlaying
                            .Connect()
                            .AddTo(disposables);

                        audioPlayer.Play();

                        return disposables;
                    });
        }
    }
}