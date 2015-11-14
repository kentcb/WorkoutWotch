namespace WorkoutWotch.Services.Contracts.State
{
    using System;
    using System.Reactive;

    public delegate IObservable<Unit> SaveCallback(IStateService stateService);

    public interface IStateService
    {
        IObservable<T> GetAsync<T>(string key);

        IObservable<Unit> SetAsync<T>(string key, T value);

        IObservable<Unit> RemoveAsync<T>(string key);

        IObservable<Unit> SaveAsync();

        IDisposable RegisterSaveCallback(SaveCallback saveCallback);
    }
}