namespace WorkoutWotch.UI.iOS.Utility
{
    using System;
    using Foundation;
    using ReactiveUI;
    using UIKit;

    // a table view source that automatically sizes its cells
    public class AutoSizeTableViewSource<TSource> : ReactiveTableViewSource<TSource>
    {
        private const float defaultEstimatedRowHeight = 30f;
        private readonly UITableView tableView;
        private readonly nfloat estimatedRowHeight;

        public AutoSizeTableViewSource(UITableView tableView, float estimatedRowHeight = defaultEstimatedRowHeight)
            : base(tableView)
        {
            this.tableView = tableView;
            this.estimatedRowHeight = estimatedRowHeight;
        }

        public AutoSizeTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<TSource> collection, NSString cellKey, Action<UITableViewCell> initializeCellAction = null, float estimatedRowHeight = defaultEstimatedRowHeight)
            : base(tableView, collection, cellKey, (float)UITableView.AutomaticDimension, initializeCellAction)
        {
            this.tableView = tableView;
            this.estimatedRowHeight = estimatedRowHeight;
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return this.estimatedRowHeight;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }
    }
}