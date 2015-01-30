namespace WorkoutWotch.UI.iOS.Utility
{
    using System;
    using System.Reactive.Disposables;
    using ReactiveUI;

    public abstract class TableViewCellBase<T> : ReactiveTableViewCell, IViewFor<T>
        where T : class
    {
        private readonly CompositeDisposable disposables;
        private T viewModel;
        private bool didSetupConstraints;

        protected TableViewCellBase(IntPtr handle)
            : base(handle)
        {
            this.disposables = new CompositeDisposable();
            this.CreateView();
            this.CreateBindings();
            this.UpdateConstraints();
        }

        public T ViewModel
        {
            get { return this.viewModel; }
            set { this.RaiseAndSetIfChanged(ref this.viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (T)value; }
        }

        protected CompositeDisposable Disposables
        {
            get { return this.disposables; }
        }

        public sealed override void UpdateConstraints()
        {
            base.UpdateConstraints();

            if (this.didSetupConstraints)
            {
                return;
            }

            this.UpdateConstraintsCore();
            this.didSetupConstraints = true;
        }

        public sealed override void LayoutSubviews()
        {
            base.LayoutSubviews();

            this.ContentView.SetNeedsLayout();
            this.ContentView.LayoutIfNeeded();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.disposables.Dispose();
            }
        }

        protected abstract void UpdateConstraintsCore();

        protected abstract void CreateView();

        protected abstract void CreateBindings();
    }
}