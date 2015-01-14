namespace WorkoutWotch.UI.iOS
{
    using System;
    using TinyIoC;
    using UIKit;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Services.Logger;

    public static class Application
    {
        public static readonly string UnhandledExceptionKey = "UnhandledException";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                UIApplication.Main(args, null, "AppDelegate");
            }
            catch (Exception ex)
            {
                HandleException(ex);
                throw;
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception exception)
        {
            new LoggerService().GetLogger(typeof(Application)).Error(exception, "Unhandled exception.");

            IStateService stateService;

            if (!TinyIoCContainer.Current.TryResolve<IStateService>(out stateService))
            {
                return;
            }

            stateService.SetAsync(UnhandledExceptionKey, exception.ToString());
        }
    }
}