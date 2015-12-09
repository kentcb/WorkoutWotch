[assembly: Xamarin.Forms.ExportRenderer(typeof(WorkoutWotch.UI.Controls.ControlButton), typeof(WorkoutWotch.UI.iOS.Renderers.ControlButtonRenderer))]

namespace WorkoutWotch.UI.iOS.Renderers
{
    using System.ComponentModel;
    using UIKit;
    using WorkoutWotch.UI.Controls;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    public sealed class ControlButtonRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && this.Control != null)
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
                case nameof(Image.Source):
                    UpdateTintColor((ControlButton)this.Element, this.Control);
                    break;
                default:
                    break;
            }
        }

        private static void UpdateTintColor(ControlButton element, UIImageView nativeView)
        {
            nativeView.TintColor = (element.IsEnabledEx ? element.EnabledTintColor : element.DisabledTintColor).ToUIColor();
            nativeView.Image = nativeView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }
    }
}