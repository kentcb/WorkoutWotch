[assembly: Xamarin.Forms.ExportRenderer(typeof(WorkoutWotch.UI.Controls.ControlButton), typeof(WorkoutWotch.UI.iOS.Renderers.ControlButtonRenderer))]

namespace WorkoutWotch.UI.iOS.Renderers
{
    using System.ComponentModel;
    using UIKit;
    using WorkoutWotch.UI.Controls;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    public sealed class ControlButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && this.Control != null)
            {
                UpdateImage((ControlButton)e.NewElement, this.Control);
                UpdateTintColor((ControlButton)this.Element, this.Control);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case nameof(ControlButton.ImageSource):
                    UpdateImage((ControlButton)this.Element, this.Control);
                    break;
                case nameof(ControlButton.IsEnabled):
                case nameof(ControlButton.EnabledTintColor):
                case nameof(ControlButton.DisabledTintColor):
                    UpdateTintColor((ControlButton)this.Element, this.Control);
                    break;
                default:
                    break;
            }
        }

        private static void UpdateImage(ControlButton element, UIButton nativeView)
        {
            var fileImageSource = (FileImageSource)element.ImageSource;

            if (fileImageSource == null)
            {
                nativeView.SetImage(null, UIControlState.Normal);
                return;
            }

            nativeView.SetImage(UIImage.FromBundle(fileImageSource.File), UIControlState.Normal);
        }

        private static void UpdateTintColor(ControlButton element, UIButton nativeView) =>
            nativeView.TintColor = (element.IsEnabled ? element.EnabledTintColor : element.DisabledTintColor).ToUIColor();
    }
}