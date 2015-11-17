namespace WorkoutWotch.UI.Controls
{
    using Xamarin.Forms;

    public class ControlButton : Button
    {
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create<ControlButton, ImageSource>(
            x => x.ImageSource,
            default(ImageSource));

        public static readonly BindableProperty EnabledTintColorProperty = BindableProperty.Create<ControlButton, Color>(
            x => x.EnabledTintColor,
            Color.Black);

        public static readonly BindableProperty DisabledTintColorProperty = BindableProperty.Create<ControlButton, Color>(
            x => x.DisabledTintColor,
            Color.Gray);

        public ImageSource ImageSource
        {
            get { return (ImageSource)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public Color EnabledTintColor
        {
            get { return (Color)this.GetValue(EnabledTintColorProperty); }
            set { this.SetValue(EnabledTintColorProperty, value); }
        }

        public Color DisabledTintColor
        {
            get { return (Color)this.GetValue(DisabledTintColorProperty); }
            set { this.SetValue(DisabledTintColorProperty, value); }
        }
    }
}