namespace WorkoutWotch.UI
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using global::ReactiveUI;
    using global::ReactiveUI.XamForms;
    using WorkoutWotch.ViewModels;
    using Xamarin.Forms;

    public partial class ExerciseProgramsView : ReactiveContentPage<ExerciseProgramsViewModel>
    {
        public ExerciseProgramsView()
        {
            InitializeComponent();

            this.activityIndicator.Opacity = 0;
            this.activityIndicator.IsVisible = false;
            this.exerciseProgramsListView.Opacity = 0;
            this.exerciseProgramsListView.IsVisible = false;

            this
                .WhenActivated(
                    disposables =>
                    {
                        this
                            .OneWayBind(this.ViewModel, x => x.Programs, x => x.exerciseProgramsListView.ItemsSource)
                            .AddTo(disposables);
                        this
                            .Bind(this.ViewModel, x => x.SelectedProgram, x => x.exerciseProgramsListView.SelectedItem)
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Status, x => x.errorLabel.Text, x => GetErrorMessage(x))
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.ParseErrorMessage, x => x.errorDetailLabel.Text)
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Programs, x => x.exerciseProgramsListView.ItemsSource)
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Status, x => x.errorLayout.IsVisible, x => IsErrorStatus(x))
                            .AddTo(disposables);

                        this
                            .WhenAnyValue(x => x.ViewModel.Status, IsLoadingStatus)
                            .DistinctUntilChanged()
                            .Select(isActive => Observable.Defer(() => this.AnimateActivityIndicatorOpacity(isActive)))
                            .Concat()
                            .SubscribeSafe()
                            .AddTo(disposables);

                        this
                            .WhenAnyValue(x => x.ViewModel.Status, x => !IsLoadingStatus(x) && !IsErrorStatus(x))
                            .DistinctUntilChanged()
                            .Select(isVisible => Observable.Defer(() => this.AnimateListViewOpacity(isVisible)))
                            .Concat()
                            .SubscribeSafe()
                            .AddTo(disposables);
                    });
        }

        private IObservable<Unit> AnimateActivityIndicatorOpacity(bool isActive) =>
            Observable
                .Start(
                    () =>
                    {
                        this.activityIndicator.IsVisible = true;
                        this.activityIndicator.IsRunning = true;
                    },
                    RxApp.MainThreadScheduler)
                .SelectMany(
                    _ =>
                        this
                            .activityIndicator
                            .FadeTo(isActive ? 1 : 0, 500, isActive ? Easing.CubicOut : Easing.CubicIn)
                            .ToObservable())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(
                    _ =>
                    {
                        if (!isActive)
                        {
                            this.activityIndicator.IsVisible = false;
                            this.activityIndicator.IsRunning = false;
                        }
                    })
                .ToSignal();

        private IObservable<Unit> AnimateListViewOpacity(bool isVisible) =>
            Observable
                .Start(
                    () =>
                    {
                        this.exerciseProgramsListView.IsVisible = true;
                    },
                    RxApp.MainThreadScheduler)
                .SelectMany(
                    _ =>
                        this
                            .exerciseProgramsListView
                            .FadeTo(isVisible ? 1 : 0, 500, isVisible ? Easing.CubicOut : Easing.CubicIn)
                            .ToObservable())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(
                    _ =>
                    {
                        if (!isVisible)
                        {
                            this.exerciseProgramsListView.IsVisible = false;
                        }
                    })
                .ToSignal();

        private static bool IsLoadingStatus(ExerciseProgramsViewModelStatus status) =>
            status == ExerciseProgramsViewModelStatus.Loading;

        private static bool IsErrorStatus(ExerciseProgramsViewModelStatus status) =>
            status == ExerciseProgramsViewModelStatus.ParseFailed || status == ExerciseProgramsViewModelStatus.LoadFailed;

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