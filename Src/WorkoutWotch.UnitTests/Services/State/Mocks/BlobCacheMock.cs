namespace WorkoutWotch.UnitTests.Services.State.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using Akavache;
    using PCLMock;

    public sealed class BlobCacheMock : MockBase<IBlobCache>, IBlobCache
    {
        public BlobCacheMock(MockBehavior behavior = MockBehavior.Strict)
            :base(behavior)
        {
        }

        public IObservable<Unit> Shutdown => this.Apply(x => x.Shutdown);

        public IScheduler Scheduler => this.Apply(x => x.Scheduler);

        public IObservable<Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?)) =>
            this.Apply(x => x.Insert(key, data, absoluteExpiration));

        public IObservable<byte[]> Get(string key) =>
            this.Apply(x => x.Get(key));

        public IObservable<IEnumerable<string>> GetAllKeys() =>
            this.Apply(x => x.GetAllKeys());

        public IObservable<DateTimeOffset?> GetCreatedAt(string key) =>
            this.Apply(x => x.GetCreatedAt(key));

        public IObservable<Unit> Flush() =>
            this.Apply(x => x.Flush());

        public IObservable<Unit> Invalidate(string key) =>
            this.Apply(x => x.Invalidate(key));

        public IObservable<Unit> InvalidateAll() =>
            this.Apply(x => x.InvalidateAll());
 
        public IObservable<Unit> Vacuum() =>
            this.Apply(x => x.Vacuum());

        public void Dispose() =>
            this.Apply(x => x.Dispose());
    }
}