namespace WorkoutWotch.UI.iOS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Akavache;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    using TinyIoC;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.UI.iOS.Views.ExercisePrograms;

    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        private UIWindow window;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            this.InitializeAppearanceManager();
            this.InitializeAkavache();
            this.InitializeIoc();
            this.ConfigureLogging();

            #if DEBUG
            // we do not want to await the task
            #pragma warning disable 4014

            this.CheckForPreviouslyUnhandledExceptions();

            #pragma warning restore 4014
            #endif

            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            var view = TinyIoCContainer.Current.Resolve<ExerciseProgramsHostView>();
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

        private void InitializeAppearanceManager()
        {
            AppearanceManager.Configure();
        }

        private void InitializeAkavache()
        {
            BlobCache.ApplicationName = "Workout Wotch";
        }

        private void InitializeIoc()
        {
            this.RegisterServices(TinyIoCContainer.Current);
            this.RegisterViewModels(TinyIoCContainer.Current);
        }

        private void ConfigureLogging()
        {
            var loggerService = TinyIoCContainer.Current.Resolve<ILoggerService>();

            #if DEBUG

            loggerService.Threshold = LogLevel.Debug;

            #else

            loggerService.Threshold = LogLevel.Info;

            #endif

            loggerService.Entries.Subscribe(x => Console.WriteLine("[{0}] {1} #{2} {3}", x.Level, x.Name, x.ThreadId, x.Message));
        }

        private async Task SaveStateAsync()
        {
            IStateService stateService;

            if (!TinyIoCContainer.Current.TryResolve<IStateService>(out stateService))
            {
                return;
            }

            await stateService.SaveAsync();
        }

        #if DEBUG

        // if the app crashed in a prior run, we may have captured the exception details and this will enable us to see those details
        private async Task CheckForPreviouslyUnhandledExceptions()
        {
            var stateService = TinyIoCContainer.Current.Resolve<IStateService>();
            var loggerService = TinyIoCContainer.Current.Resolve<ILoggerService>();

            try
            {
                var unhandledException = await stateService.GetAsync<string>(Application.UnhandledExceptionKey).ConfigureAwait(continueOnCapturedContext: false);
                loggerService.GetLogger(this.GetType()).Error("Recovered details of a previously unhandled exception: {0}", unhandledException);
                Debugger.Break();

                await stateService.RemoveAsync<string>(Application.UnhandledExceptionKey).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (KeyNotFoundException)
            {
                // swallow - no previously unhandled exception to deal with
            }
        }

        #endif
    }
}