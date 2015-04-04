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
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.ExecutionContext)
                .AddTo(this.disposables);

            this.isActive = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ExecutionContext.CurrentExercise,
                    (ec, currentExercise) => ec != null && currentExercise == this.model)
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.IsActive)
                .AddTo(this.disposables);

            // TODO: HACK: why can't I use currentExercise instead of this.ExecutionContext.CurrentExercise below?
            // Rx seems to be passing through the old exercise for currentExercise with the new value for currentExerciseProgress
            this.progressTimeSpan = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(
                    _ => this.WhenAnyValue(
                        x => x.ExecutionContext.CurrentExercise,
                        x => x.ExecutionContext.CurrentExerciseProgress,
                        (currentExercise, currentExerciseProgress) => this.ExecutionContext.CurrentExercise == this.model ? currentExerciseProgress : (TimeSpan?)null)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .StartWith(TimeSpan.Zero))
                .Switch()
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.ProgressTimeSpan)
                .AddTo(this.disposables);

            this.progress = this.WhenAny(
                    x => x.Duration,
                    x => x.ProgressTimeSpan,
                    (duration, progressTimeSpan) => progressTimeSpan.Value.TotalMilliseconds / duration.Value.TotalMilliseconds)
                .Select(x => double.IsNaN(x) || double.IsInfinity(x) ? 0d : x)
                .Select(x => Math.Min(1d, x))
                .Select(x => Math.Max(0d, x))
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.Progress)
                .AddTo(this.disposables);
        }

        public Exercise Model
        {
            get { return this.model; }
        }

        public string Name
        {
            get { return this.model.Name; }
        }

        public TimeSpan Duration
        {
            get { return this.model.Duration; }
        }

        public TimeSpan ProgressTimeSpan
        {
            get { return this.progressTimeSpan.Value; }
        }

        public double Progress
        {
            get { return this.progress.Value; }
        }

        public bool IsActive
        {
            get { return this.isActive.Value; }
        }

        private ExecutionContext ExecutionContext
        {
            get { return this.executionContext.Value; }
        }

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