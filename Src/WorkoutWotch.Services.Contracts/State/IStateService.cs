namespace WorkoutWotch.Services.Contracts.State
{
    using System;
    using System.Threading.Tasks;

    public interface IStateService
    {
        Task<T> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value);

        Task RemoveAsync<T>(string key);

        Task SaveAsync();

        IDisposable RegisterSaveCallback(Func<IStateService, Task> saveTaskFactory);
    }
}