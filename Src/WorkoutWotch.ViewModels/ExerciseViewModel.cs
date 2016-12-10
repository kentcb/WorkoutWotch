namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using ReactiveUI;
    using WorkoutWotch.Models;

    public sealed class ExerciseViewModel : ReactiveObject, ISupportsActivation
    {
        private readonly ViewModelActivator activator;
        private readonly Exercise model;
        private readonly ObservableAsPropertyHelper<ExecutionContext> executionContext;
        private readonly ObservableAsPropertyHelper<TimeSpan> progressTimeSpan;
        private readonly ObservableAsPropertyHelper<bool> isActive;
        private readonly ObservableAsPropertyHelper<double> progress;

        public ExerciseViewModel(IScheduler scheduler, Exercise model, IObservable<ExecutionContext> executionContext)
        {
            Ensure.ArgumentNotNull(scheduler, nameof(scheduler));
            Ensure.ArgumentNotNull(model, nameof(model));
            Ensure.ArgumentNotNull(executionContext, nameof(executionContext));

            this.activator = new ViewModelActivator();
            this.model = model;

            var isActivated = this
                .GetIsActivated();

            var activeExecutionContext = isActivated
                .Select(
                    activated =>
                    {
                        if (activated)
                        {
                            return executionContext;
                        }
                        else
                        {
                            return Observable<ExecutionContext>.Default;
                        }
                    })
                .Switch()
                .Publish()
                .RefCount();

            this.executionContext = activeExecutionContext
                .ToProperty(this, x => x.ExecutionContext, scheduler: scheduler);

            this.progressTimeSpan = activeExecutionContext
                .Select(
                    ec =>
                        ec == null
                            ? Observable.Return(TimeSpan.Zero)
                            : ec
                                .WhenAnyValue(x => x.CurrentExerciseProgress)
                                .Where(_ => ec.CurrentExercise == this.model)
                                .StartWith(TimeSpan.Zero))
                .Switch()
                .ToProperty(this, x => x.ProgressTimeSpan, scheduler: scheduler);

            this.progress = this
                .WhenAny(
                    x => x.Duration,
                    x => x.ProgressTimeSpan,
                    (duration, progressTimeSpan) => progressTimeSpan.Value.TotalMilliseconds / duration.Value.TotalMilliseconds)
                .Select(progressRatio => double.IsNaN(progressRatio) || double.IsInfinity(progressRatio) ? 0d : progressRatio)
                .Select(progressRatio => Math.Min(1d, progressRatio))
                .Select(progressRatio => Math.Max(0d, progressRatio))
                .ToProperty(this, x => x.Progress, scheduler: scheduler);

            this.isActive = Observable
                .CombineLatest(
                    this
                        .WhenAnyValue(x => x.ExecutionContext)
                        .Select(ec => ec == null ? Observable<TimeSpan>.Never : ec.WhenAnyValue(x => x.SkipAhead))
                        .Switch(),
                    this
                        .WhenAnyValue(x => x.ExecutionContext)
                        .Select(ec => ec == null ? Observable<Exercise>.Never : ec.WhenAnyValue(x => x.CurrentExercise))
                        .Switch(),
                    (skip, current) => skip == TimeSpan.Zero && current == this.model)
                .ToProperty(this, x => x.IsActive, scheduler: scheduler);
        }

        public ViewModelActivator Activator => this.activator;

        public Exercise Model => this.model;

        public string Name => this.model.Name;

        public TimeSpan Duration => this.model.Duration;

        public TimeSpan ProgressTimeSpan => this.progressTimeSpan.Value;

        public double Progress => this.progress.Value;

        public bool IsActive => this.isActive.Value;

        private ExecutionContext ExecutionContext => this.executionContext.Value;
    }
}