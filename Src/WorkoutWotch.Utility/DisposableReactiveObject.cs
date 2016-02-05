namespace WorkoutWotch.Utility
{
    using System;
    using System.Threading;
    using ReactiveUI;

    public abstract class DisposableReactiveObject : ReactiveObject, IDisposable
    {
        private const int DisposalNotStarted = 0;
        private const int DisposalStarted = 1;
        private const int DisposalComplete = 2;

        // see the constants defined above for valid values
        private int disposeStage;

        #if DEBUG

        ~DisposableReactiveObject()
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

        protected void VerifyNotDisposing() =>
            Ensure.Condition(!this.IsDisposing, () => new ObjectDisposedException(this.ObjectName));

        protected void VerifyNotDisposed() =>
            Ensure.Condition(!this.IsDisposed, () => new ObjectDisposedException(this.ObjectName));

        protected void VerifyNotDisposedOrDisposing() =>
            Ensure.Condition(!this.IsDisposedOrDisposing, () => new ObjectDisposedException(this.ObjectName));

        protected virtual void Dispose(bool disposing)
        {
        }

        protected virtual void OnDisposing() =>
            this.Disposing?.Invoke(this, EventArgs.Empty);

        protected void MarkAsDisposed()
        {
            GC.SuppressFinalize(this);
            Interlocked.Exchange(ref this.disposeStage, DisposalComplete);
        }
    }
}