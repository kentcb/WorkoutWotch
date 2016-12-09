namespace WorkoutWotch.Services.Android.Audio
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using global::Android.App;
    using global::Android.Media;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioService : IAudioService
    {
        public IObservable<Unit> Play(string name)
        {
            Ensure.ArgumentNotNull(name, nameof(name));

            return Observable
                .Create<Unit>(
                    observer =>
                    {
                        var disposables = new CompositeDisposable();
                        var mediaPlayer = MediaPlayer
                            .Create(Application.Context, global::Android.Net.Uri.Parse("android.resource://" + Application.Context.PackageName + "/raw/" + name.ToLowerInvariant()));
                        var subscription = Observable
                            .FromEventPattern(x => mediaPlayer.Completion += x, x => mediaPlayer.Completion -= x)
                            .FirstAsync()
                            .Select(_ => Unit.Default)
                            .Subscribe(observer)
                            .AddTo(disposables);
                        mediaPlayer.Start();

                        Disposable
                            .Create(
                                () =>
                                {
                                    // make sure we release *and* dispose, because I don't think dispose calls release :/
                                    mediaPlayer.Release();
                                    mediaPlayer.Dispose();
                                })
                            .AddTo(disposables);

                        return disposables;
                    });
        }
    }
}