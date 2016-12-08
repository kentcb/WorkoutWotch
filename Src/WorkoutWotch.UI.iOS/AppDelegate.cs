namespace WorkoutWotch.UI.iOS
{
    using Foundation;
    using Splat;
    using UIKit;

    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            var compositionRoot = new iOSCompositionRoot();
            var app = compositionRoot.ResolveApp();

            var splatRegistrar = new iOSSplatRegistrar();
            splatRegistrar.Register(Locator.CurrentMutable, compositionRoot);
            app.Initialize();

            LoadApplication(app);

            return base.FinishedLaunching(application, options);
        }
    }
}