using System;
using Akavache;
using Kent.Boogaart.PCLMock;

namespace WorkoutWotch.UnitTests.Services.State.Mocks
{
    public sealed class BlobCacheMock : MockBase<IBlobCache>, IBlobCache
    {
        public BlobCacheMock(MockBehavior behavior = MockBehavior.Strict)
            :base(behavior)
        {
        }

        #region IBlobCache implementation

        public IObservable<System.Reactive.Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return this.Apply(x => x.Insert(key, data, absoluteExpiration));
        }

        public IObservable<byte[]> GetAsync(string key)
        {
            return this.Apply(x => x.GetAsync(key));
        }

        public System.Collections.Generic.IEnumerable<string> GetAllKeys()
        {
            return this.Apply(x => x.GetAllKeys());
        }

        public IObservable<DateTimeOffset?> GetCreatedAt(string key)
        {
            return this.Apply(x => x.GetCreatedAt(key));
        }

        public IObservable<System.Reactive.Unit> Flush()
        {
            return this.Apply(x => x.Flush());
        }

        public IObservable<System.Reactive.Unit> Invalidate(string key)
        {
            return this.Apply(x => x.Invalidate(key));
        }

        public IObservable<System.Reactive.Unit> InvalidateAll()
        {
            return this.Apply(x => x.InvalidateAll());
        }

        public IObservable<System.Reactive.Unit> Shutdown
        {
            get
            {
                return this.Apply(x => x.Shutdown);
            }
        }

        public System.Reactive.Concurrency.IScheduler Scheduler
        {
            get
            {
                return this.Apply(x => x.Scheduler);
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            this.Apply(x => x.Dispose());
        }

        #endregion
    }
}

