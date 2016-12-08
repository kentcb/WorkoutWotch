namespace WorkoutWotch.UI.Android
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reactive.Linq;
    using Genesis.Logging;
    using global::Android.App;
    using global::Android.OS;
    using global::Android.Runtime;

    [Application(
        LargeHeap = true,
#if DEBUG
        Debuggable = true
#else
        Debuggable = false
#endif
        )]
    public sealed class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private static MainApplication instance;
        private readonly AndroidCompositionRoot compositionRoot;

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
            : base(handle, transer)
        {
            global::Android.Util.Log.Info(typeof(MainApplication).Name, "Starting.");

            ConfigureAmbientLoggerService();
            DirectLoggingOutputToConsole();

            instance = this;
            this.compositionRoot = new AndroidCompositionRoot();
        }

        public static MainApplication Instance => instance;

        public AndroidCompositionRoot CompositionRoot => this.compositionRoot;

        public override void OnCreate()
        {
            base.OnCreate();

            RegisterActivityLifecycleCallbacks(this);
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            //CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            //CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            //CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }

        [Conditional("LOGGING")]
        private static void ConfigureAmbientLoggerService() =>
            LoggerService.Current = new DefaultLoggerService();

        [Conditional("LOGGING")]
        private void DirectLoggingOutputToConsole() =>
            LoggerService
                .Current
                .Entries
                .SubscribeSafe(
                    entry =>
                    {
                        FormattableString message = $"#{entry.ThreadId} {entry.Message}";
                        global::Android.Util.Log.WriteLine(ToLogPriority(entry.Level), GetLastCharacters(entry.Name, 64), message.ToString(CultureInfo.InvariantCulture));
                    });

        private static global::Android.Util.LogPriority ToLogPriority(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return global::Android.Util.LogPriority.Debug;
                case LogLevel.Info:
                case LogLevel.Perf:
                    return global::Android.Util.LogPriority.Info;
                case LogLevel.Warn:
                    return global::Android.Util.LogPriority.Warn;
                case LogLevel.Error:
                    return global::Android.Util.LogPriority.Error;
                default:
                    return global::Android.Util.LogPriority.Verbose;
            }
        }

        private static string GetLastCharacters(string s, int characters)
        {
            if (s.Length <= characters)
            {
                return s;
            }

            return s.Substring(s.Length - characters - 1);
        }
    }
}