namespace ReactiveUI
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using Genesis.Ensure;

    public static class IsActivatedExtensions
    {
        public static IObservable<bool> GetIsActivated(this ISupportsActivation @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return Observable
                .Merge(
                    @this.Activator.Activated.Select(_ => true),
                    @this.Activator.Deactivated.Select(_ => false))
                .Replay(1)
                .RefCount();
        }
    }
}