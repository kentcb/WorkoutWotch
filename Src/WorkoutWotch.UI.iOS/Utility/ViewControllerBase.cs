namespace WorkoutWotch.UI.iOS.Utility
{
    using System.Reactive.Disposables;
    using ReactiveUI;

    // a base class for view controllers to save repetitive code
    public abstract class ViewControllerBase<TViewModel> : ReactiveViewController, IViewFor<TViewModel>
        where TViewModel : class
    {
        private readonly CompositeDisposable disposables;
        private TViewModel viewModel;

        protected ViewControllerBase()
        {
            this.disposables = new CompositeDisposable();
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

        protected CompositeDisposable Disposables => this.disposables;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.disposables.Dispose();
            }
        }
    }
}