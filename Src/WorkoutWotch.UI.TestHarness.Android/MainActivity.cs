namespace WorkoutWotch.UI.TestHarness.Android
{
    using global::Android.App;
    using global::Android.OS;
    using WorkoutWotch.UnitTests.Services.Logger;
    using Xunit.Runners.UI;
    using Xunit.Sdk;

    [Activity(Label = "Workout Wotch Unit Tests", MainLauncher = true, Theme = "@android:style/Theme.Material.Light")]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);

            AddTestAssembly(typeof(LoggerServiceFixture).Assembly);

            base.OnCreate(bundle);
        }
    }
}