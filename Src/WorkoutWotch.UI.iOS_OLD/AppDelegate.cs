namespace WorkoutWotch.UI.iOS
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Akavache;
    using Foundation;
    using UIKit;
    using WorkoutWotch.Services.Contracts.Logger;

    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : UIApplicationDelegate
    {
        private UIWindow window;
        private iOSCompositionRoot compositionRoot;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            this.InitializeAppearanceManager();
            this.InitializeCompositionRoot();
            this.InitializeAkavache();
            this.ConfigureLogging();
            this.InitializeControlFactory();

            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            var view = this.compositionRoot.ResolveExerciseProgramsHostView();
            var navigationController = new UINavigationController(view);

            window.RootViewController = navigationController;
            window.MakeKeyAndVisible();

            return true;
        }

        public override void DidEnterBackground(UIApplication application)
        {
            var taskId = UIApplication.SharedApplication.BeginBackgroundTask(null);

            this.SaveStateAsync()
                .ContinueWith(_ => UIApplication.SharedApplication.EndBackgroundTask(taskId));
        }

        private void InitializeAppearanceManager() =>
            AppearanceManager.Configure();

        private void InitializeCompositionRoot() =>
            this.compositionRoot = new iOSCompositionRoot();

        private void InitializeAkavache() =>
            BlobCache.ApplicationName = "Workout Wotch";

        private void ConfigureLogging()
        {
            var loggerService = this.compositionRoot.ResolveLoggerService();

#if DEBUG
            loggerService.Threshold = LogLevel.Debug;
#else
            loggerService.Threshold = LogLevel.Info;
#endif

            loggerService.Entries.Subscribe(x => Console.WriteLine("[{0}] {1} #{2} {3}", x.Level, x.Name, x.ThreadId, x.Message));
        }

        private void InitializeControlFactory() =>
            ControlFactory.Initialize(this.compositionRoot.ResolveSystemNotificationsService());

        private async Task SaveStateAsync()
        {
            var stateService = this.compositionRoot.ResolveStateService();

            await stateService.SaveAsync();
        }
    }
}