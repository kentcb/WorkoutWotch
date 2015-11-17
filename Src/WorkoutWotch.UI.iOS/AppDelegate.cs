namespace WorkoutWotch.UI.iOS
{
    using System;
    using System.Diagnostics;
    using Foundation;
    using Services.Contracts.Logger;
    using Splat;
    using UIKit;

    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            var compositionRoot = new iOSCompositionRoot();
            DirectLoggingOutputToConsole(compositionRoot.ResolveLoggerService());
            var app = compositionRoot.ResolveApp();

            var splatRegistrar = new iOSSplatRegistrar();
            splatRegistrar.Register(Locator.CurrentMutable, compositionRoot);
            app.Initialize();

            LoadApplication(app);

            return base.FinishedLaunching(application, options);
        }

        [Conditional("DEBUG")]
        private static void DirectLoggingOutputToConsole(ILoggerService loggerService) =>
            loggerService
                .Entries
                .Subscribe(entry => Console.WriteLine("[{0}] #{1} {2} : {3}", entry.Level, entry.ThreadId, entry.Name, entry.Message));
    }
}