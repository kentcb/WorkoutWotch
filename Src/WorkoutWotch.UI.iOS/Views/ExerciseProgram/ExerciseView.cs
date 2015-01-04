namespace WorkoutWotch.UI.iOS.Views.ExerciseProgram
{
    using System;
    using System.Reactive.Disposables;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    using ReactiveUI;
    using WorkoutWotch.UI.iOS.Utility;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseView : TableViewCellBase<ExerciseViewModel>
    {
        public static readonly NSString Key = new NSString("key");

        private UILabel nameLabel;
        private UILabel durationLabel;
        private UIProgressView progressView;

        public ExerciseView(IntPtr handle)
            : base(handle)
        {
        }

        protected override void CreateView()
        {
            this.BackgroundColor = Resources.ThemeLightColor;

            this.nameLabel = ControlFactory.CreateLabel().AddTo(this.Disposables);
            this.durationLabel = ControlFactory.CreateLabel().AddTo(this.Disposables);
            this.progressView = ControlFactory.CreateProgressView().AddTo(this.Disposables);

            this.ContentView.AddSubviews(this.nameLabel, this.durationLabel, this.progressView);
        }

        protected override void UpdateConstraintsCore()
        {
            this.nameLabel.SetContentCompressionResistancePriority(Layout.RequiredPriority, UILayoutConstraintAxis.Horizontal);
            this.nameLabel.SetContentCompressionResistancePriority(Layout.RequiredPriority, UILayoutConstraintAxis.Vertical);

            this.ContentView.ConstrainLayout(() =>
                this.nameLabel.Top() == this.ContentView.Top() + Layout.StandardSiblingViewSpacing &&
                this.nameLabel.Left() == this.ContentView.Left() + Layout.StandardSiblingViewSpacing &&
                this.durationLabel.Top() == this.nameLabel.Top() &&
                this.durationLabel.Height() == this.nameLabel.Height() &&
                this.durationLabel.Left() == this.nameLabel.Right() + Layout.StandardSiblingViewSpacing &&
                this.durationLabel.Right() == this.ContentView.Right() - Layout.StandardSiblingViewSpacing &&
                this.progressView.Left() == this.nameLabel.Left() &&
                this.progressView.Right() == this.durationLabel.Right() &&
                this.progressView.Top() == this.nameLabel.Bottom() + Layout.HalfSiblingViewSpacing &&
                this.progressView.Bottom() == this.ContentView.Bottom() - Layout.StandardSiblingViewSpacing);
        }

        protected override void CreateBindings()
        {
            this.OneWayBind(this.ViewModel, x => x.Name, x => x.nameLabel.Text)
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Duration, x => x.durationLabel.Text, x => x.ToString("mm\\:ss"))
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Progress, x => x.progressView.Progress)
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.IsActive, x => x.progressView.Hidden, x => !x)
                .AddTo(this.Disposables);
        }
    }
}