[assembly: Xamarin.Forms.ExportRenderer(typeof(WorkoutWotch.UI.Controls.ControlButton), typeof(WorkoutWotch.UI.Android.Renderers.ControlButtonRenderer))]

namespace WorkoutWotch.UI.Android.Renderers
{
    using System.ComponentModel;
    using WorkoutWotch.UI.Controls;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.Android;
    using Droid = global::Android.Widget;

    public sealed class ControlButtonRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                UpdateTintColor((ControlButton)this.Element, this.Control);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case nameof(ControlButton.IsEnabledEx):
                case nameof(ControlButton.EnabledTintColor):
                case nameof(ControlButton.DisabledTintColor):
                    UpdateTintColor((ControlButton)this.Element, this.Control);
                    break;
                default:
                    break;
            }
        }

        private static void UpdateTintColor(ControlButton element, Droid.ImageView nativeView)
        {
            nativeView.SetColorFilter((element.IsEnabledEx ? element.EnabledTintColor : element.DisabledTintColor).ToAndroid());
        }
    }
}