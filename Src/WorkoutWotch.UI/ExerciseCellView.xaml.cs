namespace WorkoutWotch.UI
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using global::ReactiveUI;
    using global::ReactiveUI.XamForms;
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
                            .Select(isActive => Observable.Defer(() => this.Animate(isActive)))
                            .Concat()
                            .SubscribeSafe()
                            .AddTo(disposables);
                    });
        }

        private IObservable<Unit> Animate(bool isActive)
        {
            var easing = isActive ? Easing.CubicOut : Easing.CubicIn;

            return Observable
                .Merge(
                    this.AnimateOpacity(isActive),
                    this.AnimateScale(isActive));
        }

        private IObservable<Unit> AnimateOpacity(bool isActive)
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
                .ToSignal();
        }

        private IObservable<Unit> AnimateScale(bool isActive)
        {
            if (!isActive)
            {
                return this
                    .rootLayout
                    .ScaleTo(inactiveScale, 100, Easing.CubicOut)
                    .ToObservable()
                    .ToSignal();
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
                    .ToSignal();
            }
        }
    }
}