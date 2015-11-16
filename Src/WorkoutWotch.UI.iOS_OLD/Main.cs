namespace WorkoutWotch.UI.iOS
{
    using UIKit;
    using Xamarin;

    public sealed class Application
    {
        private static void Main(string[] args)
        {
            InitializeInsights();

            Services.Analytics.AnalyticsService.Instance.Track("STARTING MOFOs");

            UIApplication.Main(args, null, "AppDelegate");
        }

        private static void InitializeInsights()
        {
            // TODO: see https://forums.xamarin.com/discussion/44825/
            Insights.HasPendingCrashReport += (sender, isStartupCrash) =>
            {
                if (isStartupCrash)
                {
                    Insights.PurgePendingCrashReports().Wait();
                }
            };

#if DEBUG
            Insights.Initialize(Insights.DebugModeKey);
#else
            Insights.Initialize("673fc5500e7da82f64c8875719066a8324e3d89d");
#endif
        }
    }
}