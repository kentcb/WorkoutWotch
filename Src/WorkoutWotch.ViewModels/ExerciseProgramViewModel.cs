namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Utility;

    public sealed class ExerciseProgramViewModel : DisposableReactiveObject
    {
        // if an exercise has progressed less that this threshold and the user skips backwards, we will skip to the prior exercise
        // otherwise, we'll return to the start of the current exercise
        private static readonly TimeSpan skipBackwardsThreshold = TimeSpan.FromMilliseconds(500);

        private readonly ILogger logger;
        private readonly ExerciseProgram model;
        private readonly CompositeDisposable disposables;
        private readonly IReadOnlyReactiveList<ExerciseViewModel> exercises;
        private readonly ObservableAsPropertyHelper<bool> isStarted;
        private readonly ObservableAsPropertyHelper<bool> isPaused;
        private readonly ObservableAsPropertyHelper<bool> isStartVisible;
        private readonly ObservableAsPropertyHelper<bool> isPauseVisible;
        private readonly ObservableAsPropertyHelper<bool> isResumeVisible;
        private readonly ObservableAsPropertyHelper<TimeSpan> progressTimeSpan;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<ExerciseViewModel> currentExercise;
        private readonly IReactiveCommand startCommand;
        private readonly IReactiveCommand<object> pauseCommand;
        private readonly IReactiveCommand<object> resumeCommand;
        private readonly IReactiveCommand skipBackwardsCommand;
        private readonly IReactiveCommand skipForwardsCommand;
        private ExecutionContext executionContext;

        public ExerciseProgramViewModel(ILoggerService loggerService, ISchedulerService schedulerService, ExerciseProgram model)
        {
            loggerService.AssertNotNull(nameof(loggerService));
            schedulerService.AssertNotNull(nameof(schedulerService));
            model.AssertNotNull(nameof(model));

            this.logger = loggerService.GetLogger(this.GetType());
            this.model = model;
            this.disposables = new CompositeDisposable();
            this.exercises = this.model.Exercises.CreateDerivedCollection(x => new ExerciseViewModel(schedulerService, x, this.WhenAnyValue(y => y.ExecutionContext)));

            this.isStarted = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x != null)
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.IsStarted)
                .AddTo(this.disposables);

            this.isPaused = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(false) : x.WhenAnyValue(y => y.IsPaused))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.IsPaused)
                .AddTo(this.disposables);

            this.progressTimeSpan = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(TimeSpan.Zero) : x.WhenAnyValue(y => y.Progress))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.ProgressTimeSpan)
                .AddTo(this.disposables);

            this.progress = this.WhenAnyValue(x => x.ProgressTimeSpan)
                .Select(x => x.TotalMilliseconds / this.model.Duration.TotalMilliseconds)
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.Progress)
                .AddTo(this.disposables);

            this.currentExercise = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ExecutionContext.CurrentExercise,
                    (ec, currentExercise) => ec == null ? null : currentExercise)
                .Select(x => this.Exercises.SingleOrDefault(y => y.Model == x))
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.CurrentExercise)
                .AddTo(this.disposables);

            var canStart = this.WhenAnyValue(x => x.IsStarted)
                .Select(x => !x);

            this.startCommand = ReactiveCommand
                .CreateAsyncObservable(canStart, this.OnStartAsync, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canPause = this.WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && !isPaused)
                .ObserveOn(schedulerService.MainScheduler);

            this.pauseCommand = ReactiveCommand.Create(canPause, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canResume = this.WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && isPaused);

            this.resumeCommand = ReactiveCommand.Create(canResume, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canSkipBackwards = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ProgressTimeSpan,
                    (ec, progress) => new { ExecutionContext = ec, Progress = progress })
                .Select(x => x.ExecutionContext != null && x.Progress >= skipBackwardsThreshold)
                .ObserveOn(schedulerService.MainScheduler);

            this.skipBackwardsCommand = ReactiveCommand.CreateAsyncObservable(canSkipBackwards, this.OnSkipBackwardsAsync, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canSkipForwards = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.CurrentExercise,
                    (ec, currentExercise) => new { ExecutionContext = ec, CurrentExercise = currentExercise })
                .Select(x => x.ExecutionContext != null && x.CurrentExercise != null && x.CurrentExercise != this.exercises.LastOrDefault())
                .ObserveOn(schedulerService.MainScheduler);

            this.skipForwardsCommand = ReactiveCommand.CreateAsyncObservable(canSkipForwards, this.OnSkipForwardsAsync, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            this.isStartVisible = this.startCommand
                .CanExecuteObservable
                .ToProperty(this, x => x.IsStartVisible)
                .AddTo(this.disposables);

            this.isPauseVisible = this.pauseCommand
                .CanExecuteObservable
                .ToProperty(this, x => x.IsPauseVisible)
                .AddTo(this.disposables);

            this.isResumeVisible = this.resumeCommand
                .CanExecuteObservable
                .ToProperty(this, x => x.IsResumeVisible)
                .AddTo(this.disposables);

            this.pauseCommand
                .Subscribe(_ => this.ExecutionContext.IsPaused = true)
                .AddTo(this.disposables);

            this.resumeCommand
                .Subscribe(_ => this.ExecutionContext.IsPaused = false)
                .AddTo(this.disposables);

            this.startCommand
                .ThrownExceptions
                .Subscribe(this.OnThrownException)
                .AddTo(this.disposables);

            this.pauseCommand
                .ThrownExceptions
                .Subscribe(this.OnThrownException)
                .AddTo(this.disposables);

            this.resumeCommand
                .ThrownExceptions
                .Subscribe(this.OnThrownException)
                .AddTo(this.disposables);

            this.skipBackwardsCommand
                .ThrownExceptions
                .Subscribe(this.OnThrownException)
                .AddTo(this.disposables);

            this.skipForwardsCommand
                .ThrownExceptions
                .Subscribe(this.OnThrownException)
                .AddTo(this.disposables);
        }

        public string Name => this.model.Name;

        public TimeSpan Duration => this.model.Duration;

        public TimeSpan ProgressTimeSpan => this.progressTimeSpan.Value;

        public double Progress => this.progress.Value;

        public IReadOnlyReactiveList<ExerciseViewModel> Exercises => this.exercises;

        public ExerciseViewModel CurrentExercise => this.currentExercise.Value;

        public bool IsStarted => this.isStarted.Value;

        public bool IsPaused => this.isPaused.Value;

        public bool IsStartVisible => this.isStartVisible.Value;

        public bool IsPauseVisible => this.isPauseVisible.Value;

        public bool IsResumeVisible => this.isResumeVisible.Value;

        public IReactiveCommand StartCommand => this.startCommand;

        public IReactiveCommand PauseCommand => this.pauseCommand;

        public IReactiveCommand ResumeCommand => this.resumeCommand;

        public IReactiveCommand SkipBackwardsCommand => this.skipBackwardsCommand;

        public IReactiveCommand SkipForwardsCommand => this.skipForwardsCommand;

        private ExecutionContext ExecutionContext
        {
            get { return this.executionContext; }
            set { this.RaiseAndSetIfChanged(ref this.executionContext, value); }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.disposables.Dispose();
            }
        }

        private IObservable<Unit> OnStartAsync(object state) =>
            this.StartAsync(((TimeSpan?)state).GetValueOrDefault(TimeSpan.Zero));

        private IObservable<Unit> OnSkipBackwardsAsync(object state) =>
            this.SkipBackwardsAsync();

        private IObservable<Unit> OnSkipForwardsAsync(object state) =>
            this.SkipForwardsAsync();

        private IObservable<Unit> StartAsync(TimeSpan skipTo = default(TimeSpan), bool isPaused = false)
        {
            this.logger.Debug("Starting {0} from {1}.", isPaused ? "paused" : "unpaused", skipTo);

            var executionContext = new ExecutionContext(skipTo)
            {
                IsPaused = isPaused
            };

            return Observable
                .Using(
                    () => executionContext,
                    ec =>
                        Observable
                            .Start(() => this.ExecutionContext = ec)
                            .SelectMany(_ => this.model.ExecuteAsync(ec)))
                .Catch<Unit, OperationCanceledException>(_ => Observable.Return(Unit.Default))
                .Do(
                    _ =>
                    {
                        this.ExecutionContext = null;
                        this.logger.Debug("Start completed.");
                    })
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }

        public IObservable<Unit> StopAsync()
        {
            this.logger.Debug("Stopping.");

            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                this.logger.Warn("Execution context is null - cannot stop.");
                return Observable.Return(Unit.Default);
            }

            executionContext.Cancel();

            return this
                .WhenAnyValue(x => x.IsStarted)
                .Where(x => !x)
                .Select(_ => Unit.Default)
                .Do(_ => this.logger.Debug("Stop completed."))
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }

        private IObservable<Unit> SkipBackwardsAsync()
        {
            this.logger.Debug("Skipping backwards.");

            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                this.logger.Warn("Execution context is null - cannot skip backwards.");
                return Observable.Return(Unit.Default);
            }

            var totalProgress = executionContext.Progress;
            var currentExerciseProgress = executionContext.CurrentExerciseProgress;
            var currentExercise = executionContext.CurrentExercise;
            var isPaused = executionContext.IsPaused;
            Exercise priorExercise = null;

            foreach (var exercise in this.exercises)
            {
                if (exercise.Model == currentExercise)
                {
                    break;
                }

                priorExercise = exercise.Model;
            }

            return this
                .StopAsync()
                .Do(
                    _ =>
                    {
                        var skipTo = totalProgress -
                            currentExerciseProgress -
                            (currentExerciseProgress < skipBackwardsThreshold && priorExercise != null ? priorExercise.Duration : TimeSpan.Zero);

                        this.StartAsync(skipTo, isPaused);
                        this.logger.Debug("Skip backwards completed.");
                    })
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }

        private IObservable<Unit> SkipForwardsAsync()
        {
            this.logger.Debug("Skipping forwards.");

            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                this.logger.Warn("Execution context is null - cannot skip forwards.");
                return Observable.Return(Unit.Default);
            }

            var totalProgress = executionContext.Progress;
            var currentExerciseProgress = executionContext.CurrentExerciseProgress;
            var currentExercise = executionContext.CurrentExercise;
            var isPaused = executionContext.IsPaused;

            return this
                .StopAsync()
                .Do(
                    _ =>
                    {
                        this.StartAsync(totalProgress - currentExerciseProgress + currentExercise.Duration, isPaused);
                        this.logger.Debug("Skip forwards completed.");
                    })
                .FirstAsync()
                .RunAsync(CancellationToken.None);
        }

        private void OnThrownException(Exception exception)
        {
            this.logger.Error(exception, "An unhandled exception occurred in a command handler.");

            // don't just swallow it - now that we've logged it, make sure it isn't ignored
            throw new InvalidOperationException("Unhandled exception in command handler.", exception);
        }
    }
}