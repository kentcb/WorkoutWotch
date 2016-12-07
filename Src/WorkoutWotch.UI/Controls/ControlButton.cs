namespace WorkoutWotch.UI.Controls
{
    using Xamarin.Forms;

    public sealed class ControlButton : Image
    {
        public static readonly BindableProperty IsEnabledExColorProperty = BindableProperty.Create(
            nameof(IsEnabledEx),
            typeof(bool),
            typeof(ControlButton),
            true);

        public static readonly BindableProperty EnabledTintColorProperty = BindableProperty.Create(
            nameof(EnabledTintColor),
            typeof(Color),
            typeof(ControlButton),
            Color.Black);

        public static readonly BindableProperty DisabledTintColorProperty = BindableProperty.Create(
            nameof(DisabledTintColor),
            typeof(Color),
            typeof(ControlButton),
            Color.Gray);

        private readonly TapGestureRecognizer tapGestureRecognizer;

        public ControlButton()
        {
            this.tapGestureRecognizer = new TapGestureRecognizer();
            this.GestureRecognizers.Add(this.tapGestureRecognizer);
        }

        public TapGestureRecognizer TapGestureRecognizer => this.tapGestureRecognizer;

        // why on Earth do we not just use IsEnabled instead of defining our own?
        // good question, Aguado
        //
        // it's because that won't work on Android thanks to this madness: https://bugzilla.xamarin.com/show_bug.cgi?id=36703
        public bool IsEnabledEx
        {
            get { return (bool)this.GetValue(IsEnabledExColorProperty); }
            set { this.SetValue(IsEnabledExColorProperty, value); }
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