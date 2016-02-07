namespace WorkoutWotch.Services.Contracts.State
{
    using System;
    using System.Reactive;

    public delegate IObservable<Unit> SaveCallback(IStateService stateService);

    public interface IStateService
    {
        IObservable<T> Get<T>(string key);

        IObservable<Unit> Set<T>(string key, T value);

        IObservable<Unit> Remove<T>(string key);

        IObservable<Unit> Save();

        IDisposable RegisterSaveCallback(SaveCallback saveCallback);
    }
}