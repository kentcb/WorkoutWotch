namespace System.Reactive.Disposables
{
    using Kent.Boogaart.HelperTrinity.Extensions;

    public static class CompositeDisposableExtensions
    {
        public static T AddTo<T>(this T @this, CompositeDisposable compositeDisposable)
            where T : IDisposable
        {
            @this.AssertGenericArgumentNotNull(nameof(@this));
            compositeDisposable.AssertNotNull(nameof(compositeDisposable));

            compositeDisposable.Add(@this);
            return @this;
        }
    }
}