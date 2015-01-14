namespace WorkoutWotch.UI.iOS.Views.ExercisePrograms
{
    using System;
    using System.Reactive.Disposables;
    using Foundation;
    using ReactiveUI;
    using UIKit;
    using WorkoutWotch.UI.iOS.Controls;
    using WorkoutWotch.UI.iOS.Utility;
    using WorkoutWotch.ViewModels;

    public class ExerciseProgramView : TableViewCellBase<ExerciseProgramViewModel>
    {
        public static readonly NSString Key = new NSString("key");
        private static readonly UIView selectedBackgroundView = new UIView { BackgroundColor = Resources.ThemeDarkColor };

        private UILabel nameLabel;
        private UILabel durationLabel;

        public ExerciseProgramView(IntPtr handle)
            : base(handle)
        {
        }

        protected override void CreateView()
        {
            this.BackgroundColor = Resources.ThemeLightColor;
            this.SelectedBackgroundView = selectedBackgroundView;
            this.AccessoryView = new CellDisclosureAccessory();

            this.nameLabel = ControlFactory
                .CreateLabel()
                .AddTo(this.Disposables);

            this.durationLabel = ControlFactory
                .CreateLabel(PreferredFont.Caption1)
                .AddTo(this.Disposables);

            this.durationLabel.TextColor = Resources.ThemeDarkColor;

            this.ContentView.AddSubviews(this.nameLabel, this.durationLabel);
        }

        protected override void UpdateConstraintsCore()
        {
            this.ContentView.ConstrainLayout(() =>
                this.nameLabel.Top() == this.ContentView.Top() + Layout.StandardSiblingViewSpacing &&
                this.nameLabel.Left() == this.ContentView.Left() + Layout.StandardSiblingViewSpacing &&
                this.nameLabel.Right() == this.ContentView.Right() - Layout.StandardSiblingViewSpacing &&
                this.durationLabel.Top() == this.nameLabel.Bottom() &&
                this.durationLabel.Bottom() == this.ContentView.Bottom() - Layout.StandardSiblingViewSpacing &&
                this.durationLabel.Left() == this.nameLabel.Left() &&
                this.durationLabel.Right() == this.nameLabel.Right());
        }

        protected override void CreateBindings()
        {
            this.OneWayBind(this.ViewModel, x => x.Name, x => x.nameLabel.Text)
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Duration, x => x.durationLabel.Text, x => x.ToString("mm\\:ss"))
                .AddTo(this.Disposables);
        }
    }
}