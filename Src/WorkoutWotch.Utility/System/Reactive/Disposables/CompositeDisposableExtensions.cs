namespace System.Reactive.Disposables
{
    using WorkoutWotch.Utility;

    public static class CompositeDisposableExtensions
    {
        public static T AddTo<T>(this T @this, CompositeDisposable compositeDisposable)
            where T : IDisposable
        {
            Ensure.GenericArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(compositeDisposable, nameof(compositeDisposable));

            compositeDisposable.Add(@this);
            return @this;
        }
    }
}