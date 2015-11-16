namespace WorkoutWotch.UI.TestHarness.iOS
{
    using UIKit;

    public class Application
    {
        static void Main(string[] args)
            => UIApplication.Main(args, null, nameof(AppDelegate));
    }
}