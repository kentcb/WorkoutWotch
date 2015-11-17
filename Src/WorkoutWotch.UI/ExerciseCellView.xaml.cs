namespace WorkoutWotch.UI
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using ReactiveUI;
    using ReactiveUI.XamForms;
    using WorkoutWotch.ViewModels;
    using Xamarin.Forms;

    public partial class ExerciseCellView : ReactiveViewCell<ExerciseViewModel>
    {
        private const double inactiveAlpha = 0.4d;
        private const double inactiveScale = 0.9d;
        private const int animationDurationMs = 300;

        public ExerciseCellView()
        {
            InitializeComponent();

            // set initial opacity/scale so that there is no temporal "active" appearance prior to the animations below kicking in
            this.rootLayout.Opacity = inactiveAlpha;
            this.progressBar.Opacity = 0;
            this.rootLayout.Scale = inactiveScale;

            this
                .WhenActivated(
                    disposables =>
                    {
                        this
                            .OneWayBind(this.ViewModel, x => x.Name, x => x.nameLabel.Text)
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Duration, x => x.durationLabel.Text, x => x.ToString("mm\\:ss"))
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Progress, x => x.progressBar.Progress)
                            .AddTo(disposables);

                        this
                            .WhenAnyValue(x => x.ViewModel.IsActive)
                            .Select(isActive => Observable.Defer(() => this.AnimateAsync(isActive)))
                            .Concat()
                            .Subscribe()
                            .AddTo(disposables);
                    });
        }

        private IObservable<Unit> AnimateAsync(bool isActive) =>
            Observable
                .Create<Unit>(
                    async observer =>
                    {
                        var easing = isActive ? Easing.CubicOut : Easing.CubicIn;

                        await Observable
                            .Merge(
                                this.AnimateOpacityAsync(isActive),
                                this.AnimateScaleAsync(isActive)
                            );

                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                .RunAsync(CancellationToken.None);

        private IObservable<Unit> AnimateOpacityAsync(bool isActive)
        {
            var easing = isActive ? Easing.CubicOut : Easing.CubicIn;

            return Observable
                .Merge(
                    this
                        .rootLayout
                        .FadeTo(isActive ? 1 : inactiveAlpha, animationDurationMs, easing)
                        .ToObservable(),
                    this
                        .progressBar
                        .FadeTo(isActive ? 1 : 0, animationDurationMs, easing)
                        .ToObservable())
                .Select(_ => Unit.Default);
        }

        private IObservable<Unit> AnimateScaleAsync(bool isActive)
        {
            if (!isActive)
            {
                return this
                    .rootLayout
                    .ScaleTo(inactiveScale, 100, Easing.CubicOut)
                    .ToObservable()
                    .Select(_ => Unit.Default);
            }
            else
            {
                return this
                    .rootLayout
                    .ScaleTo(1.03, 100, Easing.CubicOut)
                    .ToObservable()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SelectMany(
                        _ =>
                            this
                                .rootLayout
                                .ScaleTo(1, 100, Easing.CubicIn)
                                .ToObservable())
                    .Select(_ => Unit.Default);
            }
        }
    }
}