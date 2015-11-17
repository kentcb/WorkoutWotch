namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Utility;

    public sealed class ExerciseViewModel : DisposableReactiveObject
    {
        private readonly CompositeDisposable disposables;
        private readonly Exercise model;
        private readonly ObservableAsPropertyHelper<ExecutionContext> executionContext;
        private readonly ObservableAsPropertyHelper<TimeSpan> progressTimeSpan;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<bool> isActive;

        public ExerciseViewModel(ISchedulerService schedulerService, Exercise model, IObservable<ExecutionContext> executionContext)
        {
            schedulerService.AssertNotNull(nameof(schedulerService));
            model.AssertNotNull(nameof(model));
            executionContext.AssertNotNull(nameof(executionContext));

            this.disposables = new CompositeDisposable();
            this.model = model;

            this.executionContext = executionContext
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.ExecutionContext)
                .AddTo(this.disposables);

            this.isActive = Observable
                .CombineLatest(
                    this
                        .WhenAnyValue(x => x.ExecutionContext)
                        .Select(ec => ec == null ? Observable.Never<TimeSpan>() : ec.WhenAnyValue(x => x.SkipAhead))
                        .Switch(),
                    this
                        .WhenAnyValue(x => x.ExecutionContext)
                        .Select(ec => ec == null ? Observable.Never<Exercise>() : ec.WhenAnyValue(x => x.CurrentExercise))
                        .Switch(),
                    (skip, current) => skip == TimeSpan.Zero && current == this.model)
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.IsActive)
                .AddTo(this.disposables);

            this.progressTimeSpan = this
                .WhenAnyValue(x => x.ExecutionContext)
                .Select(
                    ec =>
                        ec == null
                            ? Observable.Return(TimeSpan.Zero)
                            : ec
                                .WhenAnyValue(x => x.CurrentExerciseProgress)
                                .Where(_ => ec.CurrentExercise == this.model)
                                .StartWith(TimeSpan.Zero))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.ProgressTimeSpan)
                .AddTo(this.disposables);

            this.progress = this.WhenAny(
                    x => x.Duration,
                    x => x.ProgressTimeSpan,
                    (duration, progressTimeSpan) => progressTimeSpan.Value.TotalMilliseconds / duration.Value.TotalMilliseconds)
                .Select(progressRatio => double.IsNaN(progressRatio) || double.IsInfinity(progressRatio) ? 0d : progressRatio)
                .Select(progressRatio => Math.Min(1d, progressRatio))
                .Select(progressRatio => Math.Max(0d, progressRatio))
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.Progress)
                .AddTo(this.disposables);
        }

        public Exercise Model => this.model;

        public string Name => this.model.Name;

        public TimeSpan Duration => this.model.Duration;

        public TimeSpan ProgressTimeSpan => this.progressTimeSpan.Value;

        public double Progress => this.progress.Value;

        public bool IsActive => this.isActive.Value;

        private ExecutionContext ExecutionContext => this.executionContext.Value;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.disposables.Dispose();
            }
        }
    }
}