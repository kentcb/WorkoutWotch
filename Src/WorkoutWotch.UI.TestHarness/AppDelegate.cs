namespace WorkoutWotch.UI.TestHarness
{
    using Foundation;
    using MonoTouch.NUnit.UI;
    using UIKit;
    using WorkoutWotch.UnitTests.Services.Logger;

    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;
        TouchRunner runner;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            runner = new TouchRunner(window);

            runner.Add(typeof(LoggerServiceFixture).Assembly);

            window.RootViewController = new UINavigationController(runner.GetViewController());
            window.MakeKeyAndVisible();
            
            return true;
        }
    }
}