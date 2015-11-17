[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.ListView), typeof(WorkoutWotch.UI.iOS.Renderers.ListViewRenderer))]

namespace WorkoutWotch.UI.iOS.Renderers
{
    using UIKit;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    public sealed class ListViewRenderer : Xamarin.Forms.Platform.iOS.ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            var listView = this.Control as UITableView;

            if (listView == null)
            {
                return;
            }

            listView.CellLayoutMarginsFollowReadableWidth = false;
        }
    }
}