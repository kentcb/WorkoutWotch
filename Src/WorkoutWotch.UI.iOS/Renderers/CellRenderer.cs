[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.ViewCell), typeof(WorkoutWotch.UI.iOS.Renderers.ViewCellRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.TextCell), typeof(WorkoutWotch.UI.iOS.Renderers.TextCellRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.ImageCell), typeof(WorkoutWotch.UI.iOS.Renderers.ImageCellRenderer))]

namespace WorkoutWotch.UI.iOS.Renderers
{
    using UIKit;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    public sealed class ViewCellRenderer : ViewCellBehaviorRenderer
    {
        private static readonly UIView selectedBackgroundView = new UIView { BackgroundColor = App.NavigationColor.ToUIColor() };

        public override UITableViewCell GetCell(Cell element, UITableViewCell reusableCell, UITableView tv)
        {
            var nativeView = base.GetCell(element, reusableCell, tv);
            nativeView.SelectedBackgroundView = SelectedBackgroundView.Instance;
            return nativeView;
        }
    }

    public sealed class TextCellRenderer : TextCellBehaviorRenderer
    {
        public override UITableViewCell GetCell(Cell element, UITableViewCell reusableCell, UITableView tv)
        {
            var nativeView = base.GetCell(element, reusableCell, tv);
            nativeView.SelectedBackgroundView = SelectedBackgroundView.Instance;
            return nativeView;
        }
    }

    public sealed class ImageCellRenderer : ImageCellBehaviorRenderer
    {
        public override UITableViewCell GetCell(Cell element, UITableViewCell reusableCell, UITableView tv)
        {
            var nativeView = base.GetCell(element, reusableCell, tv);
            nativeView.SelectedBackgroundView = SelectedBackgroundView.Instance;
            return nativeView;
        }
    }

    internal static class SelectedBackgroundView
    {
        public static readonly UIView Instance = new UIView { BackgroundColor = App.NavigationColor.ToUIColor() };
    }
}