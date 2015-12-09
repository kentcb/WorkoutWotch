namespace System.Reactive.Disposables
{
    using System;

    // generic variant of Rx's SerialDisposable class
    public sealed class SerialDisposable<T> : ICancelable, IDisposable
        where T : IDisposable
    {
        private readonly SerialDisposable disposable;

        public SerialDisposable()
        {
            this.disposable = new SerialDisposable();
        }

        public bool IsDisposed => this.disposable.IsDisposed;

        public T Disposable
        {
            get { return (T)this.disposable.Disposable; }
            set { this.disposable.Disposable = value; }
        }

        public void Dispose()
            => this.disposable.Dispose();
    }
}