namespace WorkoutWotch.UI.TestHarness.iOS
{
    using Foundation;
    using UIKit;
    using WorkoutWotch.UnitTests.Services.Logger;
    using Xunit.Runner;
    using Xunit.Sdk;

    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);

            AddTestAssembly(typeof(LoggerServiceFixture).Assembly);

            return base.FinishedLaunching(app, options);
        }
    }
}