namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Windows.Input;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Utility;

    public delegate ExerciseProgramViewModel ExerciseProgramViewModelFactory(ExerciseProgram model);

    public sealed class ExerciseProgramViewModel : DisposableReactiveObject, IRoutableViewModel
    {
        // if an exercise has progressed less that this threshold and the user skips backwards, we will skip to the prior exercise
        // otherwise, we'll return to the start of the current exercise
        private static readonly TimeSpan skipBackwardsThreshold = TimeSpan.FromMilliseconds(500);

        private readonly ILogger logger;
        private readonly ISchedulerService schedulerService;
        private readonly ExerciseProgram model;
        private readonly IScreen hostScreen;
        private readonly CompositeDisposable disposables;
        private readonly ReactiveCommand<TimeSpan?, Unit> startCommand;
        private readonly ReactiveCommand<Unit, Unit> pauseCommand;
        private readonly ReactiveCommand<Unit, Unit> resumeCommand;
        private readonly ReactiveCommand<Unit, Unit> skipBackwardsCommand;
        private readonly ReactiveCommand<Unit, Unit> skipForwardsCommand;
        private readonly ICommand playbackCommand;
        private IReadOnlyReactiveList<ExerciseViewModel> exercises;
        private bool isStarted;
        private bool isPaused;
        private bool isStartVisible;
        private bool isPauseVisible;
        private bool isResumeVisible;
        private TimeSpan progressTimeSpan;
        private double progress;
        private ExerciseViewModel currentExercise;
        private ExecutionContext executionContext;

        public ExerciseProgramViewModel(
            ILoggerService loggerService,
            ISchedulerService schedulerService,
            IScreen hostScreen,
            ExerciseProgram model)
        {
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(schedulerService, nameof(schedulerService));
            Ensure.ArgumentNotNull(hostScreen, nameof(hostScreen));
            Ensure.ArgumentNotNull(model, nameof(model));

            this.logger = loggerService.GetLogger(this.GetType());
            this.schedulerService = schedulerService;
            this.model = model;
            this.hostScreen = hostScreen;
            this.disposables = new CompositeDisposable();
            this.exercises = this.model.Exercises.CreateDerivedCollection(x => new ExerciseViewModel(schedulerService, x, this.WhenAnyValue(y => y.ExecutionContext)));

            this
                .WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ExecutionContext.IsCancelled,
                    (ec, isCancelled) => ec != null && !isCancelled)
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.IsStarted = x)
                .AddTo(this.disposables);

            this
                .WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(false) : x.WhenAnyValue(y => y.IsPaused))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.IsPaused = x)
                .AddTo(this.disposables);

            this
                .WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(TimeSpan.Zero) : x.WhenAnyValue(y => y.Progress))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.ProgressTimeSpan = x)
                .AddTo(this.disposables);

            this
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .Select(x => x.TotalMilliseconds / this.model.Duration.TotalMilliseconds)
                .Subscribe(x => this.Progress = x)
                .AddTo(this.disposables);

            this
                .WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ExecutionContext.CurrentExercise,
                    (ec, currentExercise) => ec == null ? null : currentExercise)
                .Select(x => this.Exercises.SingleOrDefault(y => y.Model == x))
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.CurrentExercise = x)
                .AddTo(this.disposables);

            var canStart = this
                .WhenAnyValue(x => x.IsStarted)
                .Select(x => !x);

            this.startCommand = ReactiveCommand
                .CreateFromObservable<TimeSpan?, Unit>(this.OnStartAsync, canStart, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canPause = this
                .WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && !isPaused)
                .ObserveOn(schedulerService.MainScheduler);

            this.pauseCommand = ReactiveCommand
                .CreateFromObservable(this.OnPauseAsync, canPause, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canResume = this
                .WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && isPaused)
                .ObserveOn(schedulerService.MainScheduler);

            this.resumeCommand = ReactiveCommand
                .CreateFromObservable(this.OnResumeAsync, canResume, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canSkipBackwards = this
                .WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ProgressTimeSpan,
                    (ec, progress) => new { ExecutionContext = ec, Progress = progress })
                .Select(x => x.ExecutionContext != null && x.Progress >= skipBackwardsThreshold)
                .ObserveOn(schedulerService.MainScheduler);

            this.skipBackwardsCommand = ReactiveCommand
                .CreateFromObservable(this.OnSkipBackwardsAsync, canSkipBackwards, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canSkipForwards = this
                .WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.CurrentExercise,
                    (ec, currentExercise) => new { ExecutionContext = ec, CurrentExercise = currentExercise })
                .Select(x => x.ExecutionContext != null && x.CurrentExercise != null && x.CurrentExercise != this.exercises.LastOrDefault())
                .ObserveOn(schedulerService.MainScheduler);

            this.skipForwardsCommand = ReactiveCommand
                .CreateFromObservable(this.OnSkipForwardsAsync, canSkipForwards, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            this.startCommand
                .CanExecute
                .Subscribe(x => this.IsStartVisible = x)
                .AddTo(this.disposables);

            this.pauseCommand
                .CanExecute
                .Subscribe(x => this.IsPauseVisible = x)
                .AddTo(this.disposables);

            this.resumeCommand
                .CanExecute
                .Subscribe(x => this.IsResumeVisible = x)
                .AddTo(this.disposables);

            this.startCommand
                .ThrownExceptions
                .Subscribe(ex => this.OnThrownException("start", ex))
                .AddTo(this.disposables);

            this.pauseCommand
                .ThrownExceptions
                .Subscribe(ex => this.OnThrownException("pause", ex))
                .AddTo(this.disposables);

            this.resumeCommand
                .ThrownExceptions
                .Subscribe(ex => this.OnThrownException("resume", ex))
                .AddTo(this.disposables);

            this.skipBackwardsCommand
                .ThrownExceptions
                .Subscribe(ex => this.OnThrownException("skip backwards", ex))
                .AddTo(this.disposables);

            this.skipForwardsCommand
                .ThrownExceptions
                .Subscribe(ex => this.OnThrownException("skip forwards", ex))
                .AddTo(this.disposables);

            // we don't use a reactive command here because switching in different commands causes it to get confused and
            // command binding leaves the target button disabled. We could also have not used command binding to get around
            // this problem
            this.playbackCommand = new PlaybackCommandImpl(this);

            // cancel the exercise program if the user navigates away
            this
                .hostScreen
                .Router
                .NavigationStack
                .ItemsRemoved
                .OfType<ExerciseProgramViewModel>()
                .SelectMany(x => x.StopAsync())
                .Subscribe()
                .AddTo(this.disposables);
        }

        public IScreen HostScreen => this.hostScreen;

        public string UrlPathSegment => this.Name;

        public string Name => this.model.Name;

        public TimeSpan Duration => this.model.Duration;

        public TimeSpan ProgressTimeSpan
        {
            get { return this.progressTimeSpan; }
            private set { this.RaiseAndSetIfChanged(ref this.progressTimeSpan, value); }
        }

        public double Progress
        {
            get { return this.progress; }
            private set { this.RaiseAndSetIfChanged(ref this.progress, value); }
        }

        public IReadOnlyReactiveList<ExerciseViewModel> Exercises
        {
            get { return this.exercises; }
            private set { this.RaiseAndSetIfChanged(ref this.exercises, value); }
        }

        public ExerciseViewModel CurrentExercise
        {
            get { return this.currentExercise; }
            private set { this.RaiseAndSetIfChanged(ref this.currentExercise, value); }
        }

        public bool IsStarted
        {
            get { return this.isStarted; }
            private set { this.RaiseAndSetIfChanged(ref this.isStarted, value); }
        }

        public bool IsPaused
        {
            get { return this.isPaused; }
            private set { this.RaiseAndSetIfChanged(ref this.isPaused, value); }
        }

        public bool IsStartVisible
        {
            get { return this.isStartVisible; }
            private set { this.RaiseAndSetIfChanged(ref this.isStartVisible, value); }
        }

        public bool IsPauseVisible
        {
            get { return this.isPauseVisible; }
            private set { this.RaiseAndSetIfChanged(ref this.isPauseVisible, value); }
        }

        public bool IsResumeVisible
        {
            get { return this.isResumeVisible; }
            private set { this.RaiseAndSetIfChanged(ref this.isResumeVisible, value); }
        }

        public ReactiveCommand<TimeSpan?, Unit> StartCommand => this.startCommand;

        public ReactiveCommand<Unit, Unit> PauseCommand => this.pauseCommand;

        public ReactiveCommand<Unit, Unit> ResumeCommand => this.resumeCommand;

        public ReactiveCommand<Unit, Unit> SkipBackwardsCommand => this.skipBackwardsCommand;

        public ReactiveCommand<Unit, Unit> SkipForwardsCommand => this.skipForwardsCommand;

        public ICommand PlaybackCommand => this.playbackCommand;

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

        private IObservable<Unit> OnStartAsync(TimeSpan? skipAhead) =>
            this.StartAsync(skipAhead.GetValueOrDefault(TimeSpan.Zero));

        private IObservable<Unit> OnPauseAsync() =>
            Observable
                .Start(() => this.ExecutionContext.IsPaused = true, this.schedulerService.MainScheduler)
                .Select(__ => Unit.Default);

        private IObservable<Unit> OnResumeAsync() =>
            Observable
                .Start(() => this.ExecutionContext.IsPaused = false, this.schedulerService.MainScheduler)
                .Select(__ => Unit.Default);

        private IObservable<Unit> OnSkipBackwardsAsync() =>
            this.SkipBackwardsAsync();

        private IObservable<Unit> OnSkipForwardsAsync() =>
            this.SkipForwardsAsync();

        private IObservable<Unit> StartAsync(TimeSpan skipTo = default(TimeSpan), bool isPaused = false)
        {
            this.logger.Debug("Starting {0} from {1}.", isPaused ? "paused" : "unpaused", skipTo);

            var executionContext = new ExecutionContext(skipTo)
            {
                IsPaused = isPaused
            };

            var disposables = new CompositeDisposable(
                executionContext,
                Disposable.Create(() => this.ExecutionContext = null));

            return Observable
                .Using(
                    () => disposables,
                    _ =>
                        Observable
                            .Start(() => this.ExecutionContext = executionContext, this.schedulerService.MainScheduler)
                            .SelectMany(__ => this.model.ExecuteAsync(executionContext)))
                .Catch<Unit, OperationCanceledException>(_ => Observable.Return(Unit.Default));
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
                .FirstAsync();
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

                        this.StartAsync(skipTo, isPaused).Subscribe();
                        this.logger.Debug("Skip backwards completed.");
                    })
                .FirstAsync();
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
                        this.StartAsync(totalProgress - currentExerciseProgress + currentExercise.Duration, isPaused).Subscribe();
                        this.logger.Debug("Skip forwards completed.");
                    })
                .FirstAsync();
        }

        private void OnThrownException(string source, Exception exception)
        {
            this.logger.Error(exception, "An unhandled exception occurred in the {0} command handler.", source);

            // don't just swallow it - now that we've logged it, make sure it isn't ignored
            throw new InvalidOperationException("Unhandled exception in command handler.", exception);
        }

        private sealed class PlaybackCommandImpl : ICommand
        {
            private readonly ExerciseProgramViewModel owner;

            public PlaybackCommandImpl(ExerciseProgramViewModel owner)
            {
                this.owner = owner;
            }

#pragma warning disable CS0067

            public event EventHandler CanExecuteChanged;

#pragma warning restore CS0067

            public bool CanExecute(object parameter) =>
                true;

            public void Execute(object parameter)
            {
                if (this.owner.IsStartVisible)
                {
                    this.owner.StartCommand.ExecuteAsync((TimeSpan?)parameter);
                }
                else if (this.owner.IsPauseVisible)
                {
                    this.owner.PauseCommand.ExecuteAsync();
                }
                else
                {
                    this.owner.ResumeCommand.ExecuteAsync();
                }
            }
        }
    }
}