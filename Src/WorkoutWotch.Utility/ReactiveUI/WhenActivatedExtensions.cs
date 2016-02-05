namespace ReactiveUI
{
    using System;
    using System.Reactive.Disposables;
    using WorkoutWotch.Utility;

    public static class WhenActivatedExtensions
    {
        public static IDisposable WhenActivated(
            this IActivatable @this,
            Action<CompositeDisposable> disposables,
            IViewFor view = null)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this
                .WhenActivated(
                    () =>
                    {
                        var d = new CompositeDisposable();
                        disposables(d);
                        return new[] { d };
                    },
                    view);
        }

        public static void WhenActivated(
            this ISupportsActivation @this,
            Action<CompositeDisposable> disposables)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            @this
                .WhenActivated(
                    () =>
                    {
                        var d = new CompositeDisposable();
                        disposables(d);
                        return new[] { d };
                    });
        }
    }
}