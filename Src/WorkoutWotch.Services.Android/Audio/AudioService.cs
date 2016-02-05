namespace WorkoutWotch.Services.Android.Audio
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using global::Android.App;
    using global::Android.Media;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Utility;

    public sealed class AudioService : IAudioService
    {
        public IObservable<Unit> PlayAsync(string name)
        {
            Ensure.ArgumentNotNull(name, nameof(name));

            var mediaPlayer = MediaPlayer.Create(Application.Context, global::Android.Net.Uri.Parse("android.resource://com.kent_boogaart.workoutwotch/raw/" + name.ToLowerInvariant()));
            var completed = Observable
                .FromEventPattern(x => mediaPlayer.Completion += x, x => mediaPlayer.Completion -= x)
                .Select(_ => Unit.Default)
                .RunAsync(CancellationToken.None);
            mediaPlayer.Start();

            return completed;
        }
    }
}