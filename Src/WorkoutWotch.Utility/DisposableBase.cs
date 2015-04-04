namespace WorkoutWotch.Utility
{
    using System;
    using System.Threading;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public abstract class DisposableBase : object, IDisposable
    {
        private const int DisposalNotStarted = 0;
        private const int DisposalStarted = 1;
        private const int DisposalComplete = 2;

        // see the constants defined above for valid values
        private int disposeStage;

        #if DEBUG

        ~DisposableBase()
        {
            //System.Diagnostics.Debug.WriteLine("Failed to proactively dispose of object, so it is being finalized: {0}.", this.ObjectName);
            this.Dispose(false);
        }

        #endif

        public event EventHandler Disposing;

        protected bool IsDisposing => Interlocked.CompareExchange(ref this.disposeStage, DisposalStarted, DisposalStarted) == DisposalStarted;

        protected bool IsDisposed => Interlocked.CompareExchange(ref this.disposeStage, DisposalComplete, DisposalComplete) == DisposalComplete;

        protected bool IsDisposedOrDisposing => Interlocked.CompareExchange(ref this.disposeStage, DisposalNotStarted, DisposalNotStarted) != DisposalNotStarted;

        protected virtual string ObjectName => this.GetType().FullName;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
            {
                return;
            }

            this.OnDisposing();
            this.Disposing = null;

            this.Dispose(true);
            this.MarkAsDisposed();
        }

        protected void VerifyNotDisposing()
        {
            if (this.IsDisposing)
            {
                throw new ObjectDisposedException(this.ObjectName);
            }
        }

        protected void VerifyNotDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.ObjectName);
            }
        }

        protected void VerifyNotDisposedOrDisposing()
        {
            if (this.IsDisposedOrDisposing)
            {
                throw new ObjectDisposedException(this.ObjectName);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected virtual void OnDisposing()
            => this.Disposing.Raise(this);

        protected void MarkAsDisposed()
        {
            GC.SuppressFinalize(this);
            Interlocked.Exchange(ref this.disposeStage, DisposalComplete);
        }
    }
}