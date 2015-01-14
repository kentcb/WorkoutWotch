namespace WorkoutWotch.UI.iOS.Utility
{
    using System;
    using System.Reactive.Disposables;
    using ReactiveUI;
    using TinyIoC;
    using UIKit;
    using WorkoutWotch.Services.iOS.SystemNotifications;

    // a base class for table view controllers to save repetitive code
    public abstract class TableViewControllerBase<TViewModel> : ReactiveTableViewController, IViewFor<TViewModel>
        where TViewModel : class
    {
        private readonly CompositeDisposable disposables;
        private readonly SerialDisposable dynamicTypeChangedSubscription;
        private TViewModel viewModel;

        protected TableViewControllerBase()
            : this(UITableViewStyle.Plain)
        {
        }

        protected TableViewControllerBase(UITableViewStyle style)
            : base(style)
        {
            this.disposables = new CompositeDisposable();
            this.dynamicTypeChangedSubscription = new SerialDisposable();
        }

        public TViewModel ViewModel
        {
            get { return this.viewModel; }
            set { this.RaiseAndSetIfChanged(ref this.viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (TViewModel)value; }
        }

        protected CompositeDisposable Disposables
        {
            get { return this.disposables; }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // if the dynamic type size changes, we need to reload data in the table view, otherwise things don't re-layout correctly (on device - works fine without this in simulator)
            this.dynamicTypeChangedSubscription.Disposable = TinyIoCContainer.Current
                .Resolve<ISystemNotificationsService>()
                .DynamicTypeChanged
                .Subscribe(_ => this.TableView.ReloadData());
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            this.dynamicTypeChangedSubscription.Disposable = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.dynamicTypeChangedSubscription.Dispose();
                this.disposables.Dispose();
            }
        }
    }
}