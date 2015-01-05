namespace WorkoutWotch.UI.iOS.Views.ExerciseProgram
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using MonoTouch.UIKit;
    using ReactiveUI;
    using WorkoutWotch.UI.iOS.Utility;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseProgramView : ViewControllerBase<ExerciseProgramViewModel>
    {
        private UIButton startButton;
        private UIButton pauseButton;
        private UIButton resumeButton;
        private UIButton skipBackwardsButton;
        private UIButton skipForwardsButton;
        private UIProgressView progressView;
        private ExercisesView exercisesView;

        public ExerciseProgramView()
        {
            this.EdgesForExtendedLayout = UIRectEdge.None;
        }

        public override void LoadView()
        {
            base.LoadView();

            this.View.BackgroundColor = Resources.ThemeLightColor;

            this.startButton = new ControlButton("Play")
                .AddTo(this.Disposables);

            this.pauseButton = new ControlButton("Pause")
                .AddTo(this.Disposables);

            this.resumeButton = new ControlButton("Play")
                .AddTo(this.Disposables);

            this.skipBackwardsButton = new ControlButton("SkipBackward")
                .AddTo(this.Disposables);

            this.skipForwardsButton = new ControlButton("SkipForward")
                .AddTo(this.Disposables);

            this.progressView = ControlFactory
                .CreateProgressView()
                .AddTo(this.Disposables);

            this.exercisesView = new ExercisesView(this.ViewModel);

            this.AddChildViewController(this.exercisesView);

            this.View.AddSubviews(
                this.startButton,
                this.pauseButton,
                this.resumeButton,
                this.skipBackwardsButton,
                this.skipForwardsButton,
                this.progressView,
                this.exercisesView.TableView);

            this.View.ConstrainLayout(() =>
                this.startButton.CenterX() == this.View.CenterX() &&
                this.startButton.Top() == this.View.Top() + Layout.StandardSuperviewSpacing &&
                this.pauseButton.CenterX() == this.startButton.CenterX() &&
                this.pauseButton.Top() == this.startButton.Top() &&
                this.resumeButton.CenterX() == this.startButton.CenterX() &&
                this.resumeButton.Top() == this.startButton.Top() &&
                this.skipBackwardsButton.Right() == this.startButton.Left() + Layout.StandardSiblingViewSpacing &&
                this.skipBackwardsButton.CenterY() == this.startButton.CenterY() &&
                this.skipForwardsButton.Left() == this.startButton.Right() - Layout.StandardSiblingViewSpacing &&
                this.skipForwardsButton.CenterY() == this.startButton.CenterY() &&
                this.progressView.Left() == this.View.Left() + Layout.StandardSuperviewSpacing &&
                this.progressView.Right() == this.View.Right() - Layout.StandardSuperviewSpacing &&
                this.progressView.Top() == this.startButton.Bottom() + Layout.StandardSuperviewSpacing &&
                this.progressView.Height() == 10 &&
                this.exercisesView.TableView.Left() == this.progressView.Left() &&
                this.exercisesView.TableView.Right() == this.progressView.Right() &&
                this.exercisesView.TableView.Top() == progressView.Bottom() + Layout.StandardSuperviewSpacing &&
                this.exercisesView.TableView.Bottom() == this.View.Bottom());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.OneWayBind(this.ViewModel, x => x.Progress, x => x.progressView.Progress)
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.IsStartVisible, x => x.startButton.Hidden, x => !x)
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.IsPauseVisible, x => x.pauseButton.Hidden, x => !x)
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.IsResumeVisible, x => x.resumeButton.Hidden, x => !x)
                .AddTo(this.Disposables);

            this.BindCommand(this.ViewModel, x => x.StartCommand, x => x.startButton)
                .AddTo(this.Disposables);

            this.BindCommand(this.ViewModel, x => x.PauseCommand, x => x.pauseButton)
                .AddTo(this.Disposables);

            this.BindCommand(this.ViewModel, x => x.ResumeCommand, x => x.resumeButton)
                .AddTo(this.Disposables);

            this.BindCommand(this.ViewModel, x => x.SkipBackwardsCommand, x => x.skipBackwardsButton)
                .AddTo(this.Disposables);

            this.BindCommand(this.ViewModel, x => x.SkipForwardsCommand, x => x.skipForwardsButton)
                .AddTo(this.Disposables);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x => this.NavigationItem.Title = x.Name)
                .AddTo(this.Disposables);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            this.ViewModel.StopAsync();
        }

        private sealed class ControlButton : UIButton
        {
            public ControlButton(string imageResource)
                : base(UIButtonType.System)
            {
                this.SetImage(UIImage.FromBundle(imageResource), UIControlState.Normal);
                this.UpdateTintColor();
            }

            public override bool Enabled
            {
                get
                {
                    return base.Enabled;
                }
                set
                {
                    base.Enabled = value;
                    this.UpdateTintColor();
                }
            }

            private void UpdateTintColor()
            {
                var tintColor = Resources.ThemeDarkColor;

                if (!this.Enabled)
                {
                    tintColor = Resources.DisabledColor;
                }

                this.TintColor = tintColor;
            }
        }
    }
}