namespace System.Reactive.Linq
{
    using System;
    using Genesis.Ensure;

    public static class ToSignalExtensions
    {
        public static IObservable<Unit> ToSignal<T>(this IObservable<T> @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this
                .Select(_ => Unit.Default);
        }
    }
}