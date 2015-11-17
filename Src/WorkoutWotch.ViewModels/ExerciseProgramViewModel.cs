namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Windows.Input;
    using Kent.Boogaart.HelperTrinity.Extensions;
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
        private readonly ReactiveCommand<Unit> startCommand;
        private readonly ReactiveCommand<Unit> pauseCommand;
        private readonly ReactiveCommand<Unit> resumeCommand;
        private readonly ReactiveCommand<Unit> skipBackwardsCommand;
        private readonly ReactiveCommand<Unit> skipForwardsCommand;
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
            loggerService.AssertNotNull(nameof(loggerService));
            schedulerService.AssertNotNull(nameof(schedulerService));
            hostScreen.AssertNotNull(nameof(hostScreen));
            model.AssertNotNull(nameof(model));

            this.logger = loggerService.GetLogger(this.GetType());
            this.schedulerService = schedulerService;
            this.model = model;
            this.hostScreen = hostScreen;
            this.disposables = new CompositeDisposable();
            this.exercises = this.model.Exercises.CreateDerivedCollection(x => new ExerciseViewModel(schedulerService, x, this.WhenAnyValue(y => y.ExecutionContext)));

            this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x != null)
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.IsStarted = x)
                .AddTo(this.disposables);

            this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(false) : x.WhenAnyValue(y => y.IsPaused))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.IsPaused = x)
                .AddTo(this.disposables);

            this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(TimeSpan.Zero) : x.WhenAnyValue(y => y.Progress))
                .Switch()
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.ProgressTimeSpan = x)
                .AddTo(this.disposables);

            this.WhenAnyValue(x => x.ProgressTimeSpan)
                .Select(x => x.TotalMilliseconds / this.model.Duration.TotalMilliseconds)
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.Progress = x)
                .AddTo(this.disposables);

            this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ExecutionContext.CurrentExercise,
                    (ec, currentExercise) => ec == null ? null : currentExercise)
                .Select(x => this.Exercises.SingleOrDefault(y => y.Model == x))
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.CurrentExercise = x)
                .AddTo(this.disposables);

            var canStart = this.WhenAnyValue(x => x.IsStarted)
                .Select(x => !x)
                .ObserveOn(schedulerService.MainScheduler);

            this.startCommand = ReactiveCommand
                .CreateAsyncObservable(canStart, this.OnStartAsync, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canPause = this.WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && !isPaused)
                .ObserveOn(schedulerService.MainScheduler);

            this.pauseCommand = ReactiveCommand.CreateAsyncObservable(canPause, this.OnPauseAsync, schedulerService.MainScheduler)
                .AddTo(this.disposables);

            var canResume = this.WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && isPaused)
                .ObserveOn(schedulerService.MainScheduler);

            this.resumeCommand = ReactiveCommand.CreateAsyncObservable(canResume, this.OnResumeAsync, schedulerService.MainScheduler)
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

            this.startCommand
                .CanExecuteObservable
                .Subscribe(x => this.IsStartVisible = x)
                .AddTo(this.disposables);

            this.pauseCommand
                .CanExecuteObservable
                .Subscribe(x => this.IsPauseVisible = x)
                .AddTo(this.disposables);

            this.resumeCommand
                .CanExecuteObservable
                .Subscribe(x => this.IsResumeVisible = x)
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

        public ReactiveCommand<Unit> StartCommand => this.startCommand;

        public ReactiveCommand<Unit> PauseCommand => this.pauseCommand;

        public ReactiveCommand<Unit> ResumeCommand => this.resumeCommand;

        public ReactiveCommand<Unit> SkipBackwardsCommand => this.skipBackwardsCommand;

        public ReactiveCommand<Unit> SkipForwardsCommand => this.skipForwardsCommand;

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

        private IObservable<Unit> OnStartAsync(object state) =>
            this.StartAsync(((TimeSpan?)state).GetValueOrDefault(TimeSpan.Zero));

        private IObservable<Unit> OnPauseAsync(object _) =>
            Observable
                .Start(() => this.ExecutionContext.IsPaused = true)
                .Select(__ => Unit.Default);

        private IObservable<Unit> OnResumeAsync(object _) =>
            Observable
                .Start(() => this.ExecutionContext.IsPaused = false)
                .Select(__ => Unit.Default);

        private IObservable<Unit> OnSkipBackwardsAsync(object _) =>
            this.SkipBackwardsAsync();

        private IObservable<Unit> OnSkipForwardsAsync(object _) =>
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

        private sealed class PlaybackCommandImpl : ICommand
        {
            private readonly ExerciseProgramViewModel owner;

            public PlaybackCommandImpl(ExerciseProgramViewModel owner)
            {
                this.owner = owner;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) =>
                true;

            public void Execute(object parameter)
            {
                if (this.owner.IsStartVisible)
                {
                    this.owner.StartCommand.Execute(parameter);
                }
                else if (this.owner.IsPauseVisible)
                {
                    this.owner.PauseCommand.Execute(parameter);
                }
                else
                {
                    this.owner.ResumeCommand.Execute(parameter);
                }
            }
        }
    }
}