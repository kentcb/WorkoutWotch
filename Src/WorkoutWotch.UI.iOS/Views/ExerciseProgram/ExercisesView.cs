namespace WorkoutWotch.UI.iOS.Views.ExerciseProgram
{
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using UIKit;
    using WorkoutWotch.UI.iOS.Utility;
    using WorkoutWotch.ViewModels;

    public sealed class ExercisesView : TableViewControllerBase<ExerciseProgramViewModel>
    {
        public ExercisesView(ExerciseProgramViewModel viewModel)
        {
            viewModel.AssertNotNull("viewModel");

            this.ViewModel = viewModel;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.TableView.BackgroundColor = Resources.ThemeLightColor;
            this.TableView.RegisterClassForCellReuse(typeof(ExerciseView), ExerciseView.Key);
            this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            this.TableView.AllowsSelection = false;

            this.WhenAnyValue(x => x.ViewModel.Exercises)
                .Select(x => x == null ? null : new AutoSizeTableViewSource<ExerciseViewModel>(this.TableView, x, ExerciseView.Key))
                .BindTo(this.TableView, x => x.Source)
                .AddTo(this.Disposables);
        }
    }
}