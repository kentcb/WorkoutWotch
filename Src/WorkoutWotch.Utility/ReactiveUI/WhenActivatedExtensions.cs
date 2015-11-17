namespace ReactiveUI
{
    using System;
    using System.Reactive.Disposables;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public static class WhenActivatedExtensions
    {
        public static IDisposable WhenActivated(
            this IActivatable @this,
            Action<CompositeDisposable> disposables,
            IViewFor view = null)
        {
            @this.AssertNotNull(nameof(@this));

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
            @this.AssertNotNull(nameof(@this));

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