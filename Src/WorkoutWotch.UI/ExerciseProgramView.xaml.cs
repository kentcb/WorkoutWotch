namespace WorkoutWotch.UI
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using ReactiveUI;
    using ReactiveUI.XamForms;
    using WorkoutWotch.ViewModels;
    using Xamarin.Forms;

    public partial class ExerciseProgramView : ReactiveContentPage<ExerciseProgramViewModel>
    {
        public ExerciseProgramView()
        {
            InitializeComponent();

            this
                .WhenActivated(
                    disposables =>
                    {
                        this
                            .OneWayBind(this.ViewModel, x => x.Progress, x => x.progressBar.Progress)
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Exercises, x => x.exercisesListView.ItemsSource)
                            .AddTo(disposables);
                        this
                            .BindCommand(this.ViewModel, x => x.SkipBackwardsCommand, x => x.skipBackwardButton)
                            .AddTo(disposables);
                        this
                            .BindCommand(this.ViewModel, x => x.PlaybackCommand, x => x.playbackButton)
                            .AddTo(disposables);
                        this
                            .BindCommand(this.ViewModel, x => x.SkipForwardsCommand, x => x.skipForwardButton)
                            .AddTo(disposables);

                        this
                            .WhenAnyValue(
                                x => x.ViewModel.IsStartVisible,
                                x => x.ViewModel.IsPauseVisible,
                                x => x.ViewModel.IsResumeVisible,
                                (isStartVisible, isPauseVisible, isResumeVisible) => isPauseVisible ? "Pause" : "Play")
                            .Subscribe(imageName => this.playbackButton.ImageSource = new FileImageSource { File = imageName })
                            .AddTo(disposables);

                        this
                            .skipBackwardButton
                            .Events()
                            .Clicked
                            .SelectMany(_ => this.AnimateButtonAsync(this.skipBackwardButton))
                            .Subscribe()
                            .AddTo(disposables);
                        this
                            .playbackButton
                            .Events()
                            .Clicked
                            .SelectMany(_ => this.AnimateButtonAsync(this.playbackButton))
                            .Subscribe()
                            .AddTo(disposables);
                        this
                            .skipForwardButton
                            .Events()
                            .Clicked
                            .SelectMany(_ => this.AnimateButtonAsync(this.skipForwardButton))
                            .Subscribe()
                            .AddTo(disposables);
                    });
        }

        private IObservable<Unit> AnimateButtonAsync(Button button) =>
            button
                .ScaleTo(0.95, 100, Easing.CubicOut)
                .ToObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(
                    _ =>
                        button
                            .ScaleTo(1, 150, Easing.CubicIn)
                            .ToObservable())
                .Select(_ => Unit.Default)
                .FirstAsync();
    }
}