namespace WorkoutWotch.UI.TestHarness.iOS
{
    using UIKit;

    public sealed class Application
    {
        static void Main(string[] args) =>
            UIApplication.Main(args, null, nameof(AppDelegate));
    }
}