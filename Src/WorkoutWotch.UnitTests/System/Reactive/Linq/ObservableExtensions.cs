namespace System.Reactive.Linq
{
    using System;
    using System.Collections.Generic;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public static class ObservableExtensions
    {
        public static IObservable<IList<T>> ToListAsync<T>(this IObservable<T> @this)
        {
            @this.AssertNotNull(nameof(@this));

            return @this
                .TimeoutIfTooSlow()
                .Buffer(int.MaxValue)
                .FirstAsync();
        }

        public static IObservable<T> TimeoutIfTooSlow<T>(this IObservable<T> @this)
        {
            @this.AssertNotNull(nameof(@this));

            return @this
                .Timeout(TimeSpan.FromSeconds(3));
        }
    }
}