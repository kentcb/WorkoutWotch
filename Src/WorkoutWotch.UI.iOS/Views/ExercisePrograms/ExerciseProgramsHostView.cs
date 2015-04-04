namespace WorkoutWotch.UI.iOS.Views.ExercisePrograms
{
    using System;
    using System.Reactive.Disposables;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using UIKit;
    using WorkoutWotch.UI.iOS.Utility;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseProgramsHostView : ViewControllerBase<ExerciseProgramsViewModel>
    {
        private ExerciseProgramsView exerciseProgramsView;
        private UIActivityIndicatorView activityIndicatorView;
        private UILabel errorLabel;
        private UILabel detailErrorLabel;

        public ExerciseProgramsHostView(ExerciseProgramsViewModel viewModel)
        {
            viewModel.AssertNotNull(nameof(viewModel));

            this.ViewModel = viewModel;

            this.NavigationItem.Title = "Exercise Programs";
            this.NavigationItem.HidesBackButton = true;
        }

        public override void LoadView()
        {
            base.LoadView();

            this.View.BackgroundColor = Resources.ThemeLightColor;
            this.EdgesForExtendedLayout = UIRectEdge.None;

            this.activityIndicatorView = ControlFactory
                .CreateActivityIndicator()
                .AddTo(this.Disposables);

            this.errorLabel = ControlFactory
                .CreateLabel()
                .AddTo(this.Disposables);

            this.detailErrorLabel = ControlFactory
                .CreateLabel(PreferredFont.Caption1)
                .AddTo(this.Disposables);

            this.activityIndicatorView.StartAnimating();

            this.errorLabel.TextAlignment = UITextAlignment.Center;
            this.errorLabel.Lines = 0;
            this.errorLabel.LineBreakMode = UILineBreakMode.WordWrap;

            this.detailErrorLabel.TextAlignment = UITextAlignment.Center;

            this.exerciseProgramsView = new ExerciseProgramsView(this.ViewModel);

            this.AddChildViewController(this.exerciseProgramsView);

            this.View.AddSubviews(this.errorLabel, this.detailErrorLabel, this.activityIndicatorView, this.exerciseProgramsView.TableView);

            this.View.ConstrainLayout(() =>
                this.activityIndicatorView.CenterX() == this.View.CenterX() &&
                this.activityIndicatorView.CenterY() == this.View.CenterY() &&
                this.errorLabel.Left() == this.View.Left() + Layout.StandardSuperviewSpacing &&
                this.errorLabel.Right() == this.View.Right() - Layout.StandardSuperviewSpacing &&
                this.errorLabel.CenterY() == this.View.CenterY() &&
                this.detailErrorLabel.Left() == this.errorLabel.Left() &&
                this.detailErrorLabel.Right() == this.errorLabel.Right() &&
                this.detailErrorLabel.Top() == this.errorLabel.Bottom() + Layout.StandardSiblingViewSpacing &&
                this.exerciseProgramsView.TableView.Left() == this.View.Left() &&
                this.exerciseProgramsView.TableView.Right() == this.View.Right() &&
                this.exerciseProgramsView.TableView.Top() == this.View.Top() &&
                this.exerciseProgramsView.TableView.Bottom() == this.View.Bottom());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.OneWayBind(this.ViewModel, x => x.Status, x => x.activityIndicatorView.Hidden, x => !IsLoadingStatus(x))
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Status, x => x.errorLabel.Hidden, x => !IsErrorStatus(x))
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Status, x => x.detailErrorLabel.Hidden, x => !IsErrorStatus(x))
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Status, x => x.exerciseProgramsView.TableView.Hidden, x => IsLoadingStatus(x) || IsErrorStatus(x))
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.Status, x => x.errorLabel.Text, x => GetErrorMessage(x))
                .AddTo(this.Disposables);

            this.OneWayBind(this.ViewModel, x => x.ParseErrorMessage, x => x.detailErrorLabel.Text)
                .AddTo(this.Disposables);

            this.exerciseProgramsView.NavigationRequests
                .Subscribe(x => this.NavigationController.PushViewController(x, true))
                .AddTo(this.Disposables);
        }

        private static bool IsLoadingStatus(ExerciseProgramsViewModelStatus status)
        {
            return status == ExerciseProgramsViewModelStatus.Loading;
        }

        private static bool IsErrorStatus(ExerciseProgramsViewModelStatus status)
        {
            return status == ExerciseProgramsViewModelStatus.ParseFailed || status == ExerciseProgramsViewModelStatus.LoadFailed;
        }

        private static string GetErrorMessage(ExerciseProgramsViewModelStatus status)
        {
            switch (status)
            {
                case ExerciseProgramsViewModelStatus.LoadFailed:
                    return "Failed to load exercise programs. Please ensure iCloud documents are enabled for this application under your iOS settings.";
                case ExerciseProgramsViewModelStatus.ParseFailed:
                    return "An exercise programs document was loaded, but could not be parsed.";
                default:
                    return null;
            }
        }
    }
}