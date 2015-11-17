namespace WorkoutWotch.UI.iOS.Renderers
{
    using System;
    using System.ComponentModel;
    using Behaviors;
    using UIKit;
    using Xamarin.Forms;
    using XF=Xamarin.Forms.Platform.iOS;

    public class ViewCellBehaviorRenderer : XF.ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell element, UITableViewCell reusableCell, UITableView tv)
        {
            var nativeView = base.GetCell(element, reusableCell, tv);
            CellMonitor.Monitor(element, nativeView);
            return nativeView;
        }
    }

    public class TextCellBehaviorRenderer : XF.TextCellRenderer
    {
        public override UITableViewCell GetCell(Cell element, UITableViewCell reusableCell, UITableView tv)
        {
            var nativeView = base.GetCell(element, reusableCell, tv);
            CellMonitor.Monitor(element, nativeView);
            return nativeView;
        }
    }

    public class ImageCellBehaviorRenderer : XF.ImageCellRenderer
    {
        public override UITableViewCell GetCell(Cell element, UITableViewCell reusableCell, UITableView tv)
        {
            var nativeView = base.GetCell(element, reusableCell, tv);
            CellMonitor.Monitor(element, nativeView);
            return nativeView;
        }
    }

    // monitor a Cell and related UITableViewCell without preventing them from being collected
    internal sealed class CellMonitor
    {
        private readonly WeakReference<Cell> element;
        private readonly WeakReference<UITableViewCell> nativeView;

        public static BindableProperty CellMonitorProperty = BindableProperty.CreateAttached(
            "CellMonitor",
            typeof(CellMonitor),
            typeof(CellMonitor),
            null);

        public static CellMonitor GetCellMonitor(BindableObject bindableObject) =>
            (CellMonitor)bindableObject.GetValue(CellMonitorProperty);

        public static void SetCellMonitor(BindableObject bindableObject, CellMonitor cellMonitor) =>
            bindableObject.SetValue(CellMonitorProperty, cellMonitor);

        public static void Monitor(Cell element, UITableViewCell nativeView)
        {
            var monitor = GetCellMonitor(element);
            
            if (monitor == null || !monitor.IsAlive)
            {
                monitor = new CellMonitor(element, nativeView);
            }

            SetCellMonitor(element, monitor);
        }

        public CellMonitor(Cell element, UITableViewCell nativeView)
        {
            this.element = new WeakReference<Cell>(element);
            this.nativeView = new WeakReference<UITableViewCell>(nativeView);

            element.PropertyChanged += OnElementPropertyChanged;

            UpdateAccessoryType(element, nativeView);
            UpdateSelectionType(element, nativeView);
        }

        public bool IsAlive
        {
            get
            {
                Cell element;
                UITableViewCell nativeView;

                return this.element.TryGetTarget(out element) && this.nativeView.TryGetTarget(out nativeView);
            }
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var element = (Cell)sender;
            UITableViewCell nativeView;

            if (!this.nativeView.TryGetTarget(out nativeView))
            {
                return;
            }

            if (e.PropertyName == CellBehavior.AccessoryProperty.PropertyName)
            {
                UpdateAccessoryType(element, nativeView);
            }
            else if (e.PropertyName == CellBehavior.IsSelectableProperty.PropertyName)
            {
                UpdateSelectionType(element, nativeView);
            }
        }

        private static void UpdateAccessoryType(Cell element, UITableViewCell nativeView)
        {
            var accessoryType = CellBehavior.GetAccessory(element);

            switch (accessoryType)
            {
                case AccessoryType.HasChildView:
                    nativeView.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    nativeView.AccessoryView = new CellDisclosureAccessory();
                    break;
                default:
                    nativeView.Accessory = UITableViewCellAccessory.None;
                    break;
            }
        }

        private static void UpdateSelectionType(Cell element, UITableViewCell nativeView)
        {
            var isSelectable = CellBehavior.GetIsSelectable(element);
            nativeView.SelectionStyle = isSelectable ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
        }
    }
}