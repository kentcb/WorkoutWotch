[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.ProgressBar), typeof(WorkoutWotch.UI.iOS.Renderers.ProgressBarRenderer))]

namespace WorkoutWotch.UI.iOS.Renderers
{
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    public sealed class ProgressBarRenderer : Xamarin.Forms.Platform.iOS.ProgressBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && this.Control != null)
            {
                this.Control.TrackTintColor = App.NavigationColor.ToUIColor();
                this.Control.ProgressTintColor = App.ForegroundColor.ToUIColor();
            }
        }
    }
}