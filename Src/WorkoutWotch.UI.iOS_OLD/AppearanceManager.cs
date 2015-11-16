namespace WorkoutWotch.UI.iOS
{
    using UIKit;

    // set up default appearances for controls wherever the appearance proxy functionality in iOS allows (often it doesn't, because Apple)
    internal static class AppearanceManager
    {
        public static void Configure()
        {
            // status bar
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);

            // UINavigationBar
            UINavigationBar.Appearance.TintColor = Resources.ThemeLightColor;
            UINavigationBar.Appearance.BarTintColor = Resources.ThemeDarkColor;
            UINavigationBar.Appearance.SetTitleTextAttributes(
                new UITextAttributes
            {
                TextColor = Resources.ThemeLightColor,
                TextShadowColor = Resources.ThemeDarkestColor,
                TextShadowOffset = new UIOffset(-1, 1)
            });

            // UIButton
            UIButton.Appearance.SetTitleColor(Resources.ThemeDarkestColor, UIControlState.Normal);
            UIButton.Appearance.SetTitleColor(Resources.DisabledColor, UIControlState.Disabled);
            UIButton.Appearance.SetTitleColor(Resources.ThemeDarkColor, UIControlState.Highlighted);
            UIButton.Appearance.SetTitleColor(Resources.ThemeDarkColor, UIControlState.Selected);

            // UIProgressView
            UIProgressView.Appearance.TrackTintColor = Resources.ThemeDarkColor;
            UIProgressView.Appearance.ProgressTintColor = Resources.ThemeDarkestColor;

            // UIActivityIndicatorView
            UIActivityIndicatorView.Appearance.Color = Resources.ThemeDarkestColor;
        }
    }
}