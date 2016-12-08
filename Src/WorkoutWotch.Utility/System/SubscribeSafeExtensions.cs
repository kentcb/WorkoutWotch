namespace System
{
    using Diagnostics;
    using Genesis.Ensure;
    using Genesis.Logging;
    using Runtime.CompilerServices;

    public static class SubscribeSafeExtensions
    {
#if DEBUG
        public static IDisposable SubscribeSafe<T>(
            this IObservable<T> @this,
            [CallerMemberName]string callerMemberName = null,
            [CallerFilePath]string callerFilePath = null,
            [CallerLineNumber]int callerLineNumber = 0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            return @this
                .Subscribe(
                    _ => { },
                    ex =>
                    {
                        var logger = LoggerService.GetLogger(typeof(SubscribeSafeExtensions));
                        logger.Error(ex, "An exception went unhandled. Caller member name: '{0}', caller file path: '{1}', caller line number: {2}.", callerMemberName, callerFilePath, callerLineNumber);

                        Debugger.Break();
                    });
        }

        public static IDisposable SubscribeSafe<T>(
            this IObservable<T> @this,
            Action<T> onNext,
            [CallerMemberName]string callerMemberName = null,
            [CallerFilePath]string callerFilePath = null,
            [CallerLineNumber]int callerLineNumber = 0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(onNext, nameof(onNext));

            return @this
                .Subscribe(
                    onNext,
                    ex =>
                    {
                        var logger = LoggerService.GetLogger(typeof(SubscribeSafeExtensions));
                        logger.Error(ex, "An exception went unhandled. Caller member name: '{0}', caller file path: '{1}', caller line number: {2}.", callerMemberName, callerFilePath, callerLineNumber);

                        Debugger.Break();
                    });
        }
#else
        public static IDisposable SubscribeSafe<T>(this IObservable<T> @this) =>
            @this.Subscribe();

        public static IDisposable SubscribeSafe<T>(this IObservable<T> @this, Action<T> onNext) =>
            @this.Subscribe(onNext);
#endif
    }
}