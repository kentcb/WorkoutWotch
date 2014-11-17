namespace System.Reactive.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public static class ObservableExtensions
    {
        public static IObservable<IList<T>> ToListAsync<T>(this IObservable<T> @this, TimeSpan? timeout = null)
        {
            @this.AssertNotNull("@this");

            return @this.Timeout(timeout.GetValueOrDefault(TimeSpan.FromSeconds(3)))
                .Buffer(int.MaxValue)
                .FirstAsync();
        }
    }
}