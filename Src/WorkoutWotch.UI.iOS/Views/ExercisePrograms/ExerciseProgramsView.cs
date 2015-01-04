namespace WorkoutWotch.UI.iOS.Views.ExercisePrograms
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using MonoTouch.UIKit;
    using ReactiveUI;
    using TinyIoC;
    using WorkoutWotch.UI.iOS.Utility;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseProgramsView : TableViewControllerBase<ExerciseProgramsViewModel>
    {
        private IObservable<UIViewController> navigationRequests;

        public ExerciseProgramsView(ExerciseProgramsViewModel viewModel)
        {
            viewModel.AssertNotNull("viewModel");

            this.ViewModel = viewModel;
        }

        public IObservable<UIViewController> NavigationRequests
        {
            get { return this.navigationRequests; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.navigationRequests = this.WhenAnyValue(x => x.ViewModel.SelectedProgram)
                .Select(x =>
                    {
                        if (x == null)
                        {
                            return null;
                        }

                        var view = TinyIoCContainer.Current.Resolve<Views.ExerciseProgram.ExerciseProgramView>();
                        view.ViewModel = x;
                        return view;
                    })
                .Where(x => x != null);

            this.WhenAnyValue(x => x.ViewModel.SelectedProgram)
                .Where(x => x == null)
                .Subscribe(x => this.TableView.DeselectRow(this.TableView.IndexPathForSelectedRow, false))
                .AddTo(this.Disposables);

            this.TableView.DelaysContentTouches = false;
            this.TableView.BackgroundColor = Resources.ThemeLightColor;
            this.TableView.RegisterClassForCellReuse(typeof(ExerciseProgramView), ExerciseProgramView.Key);
            this.TableView.SeparatorColor = Resources.ThemeDarkColor;
            this.TableView.TableFooterView = new UIView();

            var tableViewSource = this.WhenAnyValue(x => x.ViewModel.Programs)
                .Select(x => x == null ? null : new AutoSizeTableViewSource<ExerciseProgramViewModel>(this.TableView, x, ExerciseProgramView.Key))
                .Publish();

            tableViewSource
                .BindTo(this.TableView, x => x.Source)
                .AddTo(this.Disposables);

            tableViewSource
                .Where(x => x != null)
                .Select(x => x.ElementSelected)
                .Switch()
                .Cast<ExerciseProgramViewModel>()
                .Subscribe(x => this.ViewModel.SelectedProgram = x)
                .AddTo(this.Disposables);

            tableViewSource
                .Connect()
                .AddTo(this.Disposables);
        }

        public override void ViewWillAppear(bool animated)
        {
            this.ViewModel.SelectedProgram = null;
            base.ViewWillAppear(animated);
        }
    }
}