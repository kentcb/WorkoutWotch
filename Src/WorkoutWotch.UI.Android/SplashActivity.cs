namespace WorkoutWotch.UI.Android
{
    using global::Android.App;
    using global::Android.OS;

    [Activity(
        Theme = "@style/Splash",
        MainLauncher = true,
        NoHistory = true)]
    public sealed class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.StartActivity(typeof(MainActivity));
        }
    }
}