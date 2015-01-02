namespace WorkoutWotch.UI.iOS.Utility
{
    using System;
    using System.Drawing;
    using System.Reactive.Linq;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    using ReactiveUI;
    using TinyIoC;
    using WorkoutWotch.Services.iOS.SystemNotifications;

    // a table view source that automatically sizes its cells. Once a size for a section is determined, all cells will take on that size (unless dynamic type size is changed, in which case the cached size is invalidated)
    // if cells need to be of varying sizes, use VariableRowHightTableViewSource instead
    public class AutoSizeTableViewSource<TSource> : ReactiveTableViewSource<TSource>
    {
        private readonly UITableView tableView;
        private readonly IDisposable dataSubscription;
        private readonly IDisposable dynamicTypeChangedSubscription;
        private float?[] heights;

        public AutoSizeTableViewSource(UITableView tableView)
            : base(tableView)
        {
            this.tableView = tableView;
            this.dataSubscription = this.InitDataSubscription();
            this.dynamicTypeChangedSubscription = this.InitDynamicTypeChangedSubscription();
        }

        public AutoSizeTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<TSource> collection, NSString cellKey, Action<UITableViewCell> initializeCellAction = null)
            : base(tableView, collection, cellKey, UITableView.AutomaticDimension, initializeCellAction)
        {
            this.tableView = tableView;
            this.dataSubscription = this.InitDataSubscription();
            this.dynamicTypeChangedSubscription = this.InitDynamicTypeChangedSubscription();
        }

        public override float EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            this.ValidateSection(indexPath.Section);

            if (this.heights[indexPath.Section].HasValue)
            {
                return this.heights[indexPath.Section].Value;
            }

            var viewModel = this.ItemAt(indexPath);
            var section = this.Data[indexPath.Section];
            var cellKey = section.CellKeySelector(viewModel);
            var offscreenCell = this.tableView.DequeueReusableCell(cellKey);

            if (section.InitializeCellAction != null)
            {
                section.InitializeCellAction(offscreenCell);
            }

            var castCell = offscreenCell as IViewFor;

            if (castCell != null)
            {
                castCell.ViewModel = viewModel;
            }

            offscreenCell.SetNeedsUpdateConstraints();
            offscreenCell.UpdateConstraintsIfNeeded();

            offscreenCell.Bounds = new RectangleF(0, 0, this.tableView.Bounds.Width, this.tableView.Bounds.Height);

            offscreenCell.SetNeedsLayout();
            offscreenCell.LayoutIfNeeded();

            var height = offscreenCell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height;
            height += 1;

            this.heights[indexPath.Section] = height;

            return height;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.dataSubscription.Dispose();
                this.dynamicTypeChangedSubscription.Dispose();
            }
        }

        private IDisposable InitDataSubscription()
        {
            return this
                .WhenAnyValue(x => x.Data)
                .Select(x => new float?[x == null ? 0 : x.Count])
                .Subscribe(x => this.heights = x);
        }

        private IDisposable InitDynamicTypeChangedSubscription()
        {
            return TinyIoCContainer.Current
                .Resolve<ISystemNotificationsService>()
                .DynamicTypeChanged
                .Where(_ => this.heights != null)
                .Subscribe(
                    _ =>
                    {
                        // clear out the cached heights for every section
                        for (var i = 0; i < this.heights.Length; ++i)
                        {
                            this.heights[i] = null;
                        }
                    });
        }

        private void ValidateSection(int section)
        {
            if (section >= this.Data.Count)
            {
                throw new InvalidOperationException("Section " + section + " does not exist.");
            }
        }
    }
}