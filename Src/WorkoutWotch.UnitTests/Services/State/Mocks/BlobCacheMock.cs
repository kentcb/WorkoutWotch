namespace WorkoutWotch.UnitTests.Services.State.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using Akavache;
    using Kent.Boogaart.PCLMock;

    public sealed class BlobCacheMock : MockBase<IBlobCache>, IBlobCache
    {
        public BlobCacheMock(MockBehavior behavior = MockBehavior.Strict)
            :base(behavior)
        {
        }

        public IObservable<Unit> Shutdown
        {
            get { return this.Apply(x => x.Shutdown); }
        }

        public IScheduler Scheduler
        {
            get { return this.Apply(x => x.Scheduler); }
        }

        public IObservable<Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return this.Apply(x => x.Insert(key, data, absoluteExpiration));
        }

        public IObservable<byte[]> GetAsync(string key)
        {
            return this.Apply(x => x.GetAsync(key));
        }

        public IEnumerable<string> GetAllKeys()
        {
            return this.Apply(x => x.GetAllKeys());
        }

        public IObservable<DateTimeOffset?> GetCreatedAt(string key)
        {
            return this.Apply(x => x.GetCreatedAt(key));
        }

        public IObservable<Unit> Flush()
        {
            return this.Apply(x => x.Flush());
        }

        public IObservable<Unit> Invalidate(string key)
        {
            return this.Apply(x => x.Invalidate(key));
        }

        public IObservable<Unit> InvalidateAll()
        {
            return this.Apply(x => x.InvalidateAll());
        }

        public void Dispose()
        {
            this.Apply(x => x.Dispose());
        }
    }
}