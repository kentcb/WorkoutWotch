namespace WorkoutWotch.Services.Contracts.Audio
{
    using System;
    using System.Reactive;

    public interface IAudioService
    {
        IObservable<Unit> PlayAsync(string name);
    }
}