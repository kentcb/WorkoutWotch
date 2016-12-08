namespace WorkoutWotch.Services.State
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Akavache;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Utility;

    public sealed class StateService : IStateService
    {
        private readonly IBlobCache blobCache;
        private readonly ILogger logger;
        private readonly object sync;
        private volatile IImmutableList<SaveCallback> saveCallbacks;

        public StateService(IBlobCache blobCache, ILoggerService loggerService)
        {
            Ensure.ArgumentNotNull(blobCache, nameof(blobCache));
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));

            this.blobCache = blobCache;
            this.logger = loggerService.GetLogger(this.GetType());
            this.saveCallbacks = ImmutableList<SaveCallback>.Empty;
            this.sync = new object();
        }

        public IObservable<T> Get<T>(string key)
        {
            Ensure.ArgumentNotNull(key, nameof(key));

            return this
                .blobCache
                .GetObject<T>(key);
        }

        public IObservable<Unit> Set<T>(string key, T value)
        {
            Ensure.ArgumentNotNull(key, nameof(key));

            return this
                .blobCache
                .InsertObject<T>(key, value);
        }

        public IObservable<Unit> Remove<T>(string key)
        {
            Ensure.ArgumentNotNull(key, nameof(key));

            return this
                .blobCache
                .InvalidateObject<T>(key);
        }

        public IObservable<Unit> Save()
        {
            IObservable<IList<Unit>> saves;

            lock (this.sync)
            {
                saves = Observable
                    .CombineLatest(
                        this
                            .saveCallbacks
                            .Select(x => x(this))
                            .Where(x => x != null)
                            .DefaultIfEmpty(Observable.Return(Unit.Default))
                            .ToList());
            }

            return saves
                .Select(_ => Unit.Default)
                .Catch(
                    (Exception ex) =>
                    {
                        this.logger.Error(ex, "Failed to save.");
                        return Observable.Return(Unit.Default);
                    });
        }

        public IDisposable RegisterSaveCallback(SaveCallback saveCallback)
        {
            Ensure.ArgumentNotNull(saveCallback, nameof(saveCallback));

            lock (this.sync)
            {
                this.saveCallbacks = this.saveCallbacks.Add(saveCallback);
            }

            return new RegistrationHandle(this, saveCallback);
        }

        private void UnregisterSaveCallback(SaveCallback saveCallback)
        {
            Debug.Assert(saveCallback != null);

            lock (this.sync)
            {
                this.saveCallbacks = this.saveCallbacks.Remove(saveCallback);
            }
        }

        private sealed class RegistrationHandle : DisposableBase
        {
            private readonly StateService owner;
            private readonly SaveCallback saveCallback;

            public RegistrationHandle(StateService owner, SaveCallback saveCallback)
            {
                Debug.Assert(owner != null);
                Debug.Assert(saveCallback != null);

                this.owner = owner;
                this.saveCallback = saveCallback;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    this.owner.UnregisterSaveCallback(this.saveCallback);
                }
            }
        }
    }
}