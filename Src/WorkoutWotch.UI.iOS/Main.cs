namespace WorkoutWotch.UI.iOS
{
    using UIKit;

    public static class Application
    {
        private static void Main(string[] args) =>
            UIApplication.Main(args, null, nameof(AppDelegate));
    }
}