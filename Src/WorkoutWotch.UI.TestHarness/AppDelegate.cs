namespace WorkoutWotch.UI.TestHarness
{
    using System;
    using Foundation;
    using UIKit;
    using WorkoutWotch.UnitTests.Services.Logger;
    using Xunit.Runner;
    using Xunit.Sdk;

    [Register("AppDelegate")]
    public partial class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);

            AddTestAssembly(typeof(LoggerServiceFixture).Assembly);

            this.Writer = Console.Out;

            #if false

            // start running the test suites as soon as the application is loaded
            AutoStart = true;
            // crash the application (to ensure it's ended) and return to springboard
            TerminateAfterExecution = true;
            #endif

            return base.FinishedLaunching(app, options);
        }
    }
}