namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows.Input;
    using Genesis.Ensure;
    using Genesis.Logging;
    using ReactiveUI;
    using WorkoutWotch.Models;

    public delegate ExerciseProgramViewModel ExerciseProgramViewModelFactory(ExerciseProgram model);

    public sealed class ExerciseProgramViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        // if an exercise has progressed less that this threshold and the user skips backwards, we will skip to the prior exercise
        // otherwise, we'll return to the start of the current exercise
        private static readonly TimeSpan skipBackwardsThreshold = TimeSpan.FromMilliseconds(500);

        private readonly ViewModelActivator activator;
        private readonly ILogger logger;
        private readonly IScheduler scheduler;
        private readonly ExerciseProgram model;
        private readonly IScreen hostScreen;
        private readonly ReactiveCommand<TimeSpan?, Unit> startCommand;
        private readonly ReactiveCommand<Unit, Unit> pauseCommand;
        private readonly ReactiveCommand<Unit, Unit> resumeCommand;
        private readonly ReactiveCommand<Unit, Unit> skipBackwardsCommand;
        private readonly ReactiveCommand<Unit, Unit> skipForwardsCommand;
        private readonly ICommand playbackCommand;
        private readonly IReadOnlyReactiveList<ExerciseViewModel> exercises;
        private readonly ObservableAsPropertyHelper<bool> isStarted;
        private readonly ObservableAsPropertyHelper<bool> isPaused;
        private readonly ObservableAsPropertyHelper<bool> isStartVisible;
        private readonly ObservableAsPropertyHelper<bool> isPauseVisible;
        private readonly ObservableAsPropertyHelper<bool> isResumeVisible;
        private readonly ObservableAsPropertyHelper<TimeSpan> progressTimeSpan;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<ExerciseViewModel> currentExercise;
        private ExecutionContext executionContext;

        public ExerciseProgramViewModel(
            IScheduler scheduler,
            IScreen hostScreen,
            ExerciseProgram model)
        {
            Ensure.ArgumentNotNull(scheduler, nameof(scheduler));
            Ensure.ArgumentNotNull(hostScreen, nameof(hostScreen));
            Ensure.ArgumentNotNull(model, nameof(model));

            this.logger = LoggerService.GetLogger(this.GetType());

            using (this.logger.Perf("Construction"))
            {
                this.activator = new ViewModelActivator();
                this.scheduler = scheduler;
                this.model = model;
                this.hostScreen = hostScreen;
                this.exercises = this.model.Exercises.CreateDerivedCollection(x => new ExerciseViewModel(scheduler, x, this.WhenAnyValue(y => y.ExecutionContext)));

                this.isStarted = this
                    .WhenAnyValue(
                        x => x.ExecutionContext,
                        x => x.ExecutionContext.IsCancelled,
                        (ec, isCancelled) => ec != null && !isCancelled)
                    .ToProperty(this, x => x.IsStarted, scheduler: scheduler);

                this.isPaused = this
                    .WhenAnyValue(x => x.ExecutionContext)
                    .Select(x => x == null ? Observables.False : x.WhenAnyValue(y => y.IsPaused))
                    .Switch()
                    .ToProperty(this, x => x.IsPaused, scheduler: scheduler);

                this.progressTimeSpan = this
                    .WhenAnyValue(x => x.ExecutionContext)
                    .Select(x => x == null ? Observable.Return(TimeSpan.Zero) : x.WhenAnyValue(y => y.Progress))
                    .Switch()
                    .ToProperty(this, x => x.ProgressTimeSpan, scheduler: scheduler);

                this.progress = this
                    .WhenAnyValue(x => x.ProgressTimeSpan)
                    .Select(x => x.TotalMilliseconds / this.model.Duration.TotalMilliseconds)
                    .ToProperty(this, x => x.Progress);

                this.currentExercise = this
                    .WhenAnyValue(
                        x => x.ExecutionContext,
                        x => x.ExecutionContext.CurrentExercise,
                        (ec, currentExercise) => ec == null ? null : currentExercise)
                    .Select(x => this.Exercises.SingleOrDefault(y => y.Model == x))
                    .ToProperty(this, x => x.CurrentExercise, scheduler: scheduler);

                var canStart = this
                    .WhenAnyValue(x => x.IsStarted)
                    .Select(x => !x);

                this.startCommand = ReactiveCommand
                    .CreateFromObservable<TimeSpan?, Unit>(this.OnStart, canStart, scheduler);

                var canPause = this
                    .WhenAnyValue(x => x.IsStarted)
                    .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && !isPaused)
                    .ObserveOn(scheduler);

                this.pauseCommand = ReactiveCommand
                    .CreateFromObservable(this.OnPause, canPause, scheduler);

                var canResume = this
                    .WhenAnyValue(x => x.IsStarted)
                    .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && isPaused)
                    .ObserveOn(scheduler);

                this.resumeCommand = ReactiveCommand
                    .CreateFromObservable(this.OnResume, canResume, scheduler);

                var canSkipBackwards = this
                    .WhenAnyValue(
                        x => x.ExecutionContext,
                        x => x.ProgressTimeSpan,
                        (ec, progress) => new { ExecutionContext = ec, Progress = progress })
                    .Select(x => x.ExecutionContext != null && x.Progress >= skipBackwardsThreshold)
                    .ObserveOn(scheduler);

                this.skipBackwardsCommand = ReactiveCommand
                    .CreateFromObservable(this.OnSkipBackwards, canSkipBackwards, scheduler);

                var canSkipForwards = this
                    .WhenAnyValue(
                        x => x.ExecutionContext,
                        x => x.CurrentExercise,
                        (ec, currentExercise) => new { ExecutionContext = ec, CurrentExercise = currentExercise })
                    .Select(x => x.ExecutionContext != null && x.CurrentExercise != null && x.CurrentExercise != this.exercises.LastOrDefault())
                    .ObserveOn(scheduler);

                this.skipForwardsCommand = ReactiveCommand
                    .CreateFromObservable(this.OnSkipForwards, canSkipForwards, scheduler);

                this.isStartVisible = this
                    .startCommand
                    .CanExecute
                    .ToProperty(this, x => x.IsStartVisible);

                this.isPauseVisible = this
                    .pauseCommand
                    .CanExecute
                    .ToProperty(this, x => x.IsPauseVisible);

                this.isResumeVisible = this
                    .resumeCommand
                    .CanExecute
                    .ToProperty(this, x => x.IsResumeVisible);

                this.startCommand
                    .ThrownExceptions
                    .SubscribeSafe(ex => this.OnThrownException("start", ex));

                this.pauseCommand
                    .ThrownExceptions
                    .SubscribeSafe(ex => this.OnThrownException("pause", ex));

                this.resumeCommand
                    .ThrownExceptions
                    .SubscribeSafe(ex => this.OnThrownException("resume", ex));

                this.skipBackwardsCommand
                    .ThrownExceptions
                    .SubscribeSafe(ex => this.OnThrownException("skip backwards", ex));

                this.skipForwardsCommand
                    .ThrownExceptions
                    .SubscribeSafe(ex => this.OnThrownException("skip forwards", ex));

                // we don't use a reactive command here because switching in different commands causes it to get confused and
                // command binding leaves the target button disabled. We could also have not used command binding to get around
                // this problem
                this.playbackCommand = new PlaybackCommandImpl(this);

                this
                    .WhenActivated(
                        disposables =>
                        {
                            using (this.logger.Perf("Activation"))
                            {
                                // cancel the exercise program if the user navigates away
                                this
                                    .hostScreen
                                    .Router
                                    .NavigationStack
                                    .ItemsRemoved
                                    .OfType<ExerciseProgramViewModel>()
                                    .SelectMany(x => x.Stop())
                                    .SubscribeSafe()
                                    .AddTo(disposables);
                            }
                        });
            }
        }

        public ViewModelActivator Activator => this.activator;

        public IScreen HostScreen => this.hostScreen;

        public string UrlPathSegment => this.Name;

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

        private IObservable<Unit> OnStart(TimeSpan? skipAhead) =>
            this.Start(skipAhead.GetValueOrDefault(TimeSpan.Zero));

        private IObservable<Unit> OnPause() =>
            Observable
                .Start(() => this.ExecutionContext.IsPaused = true, this.scheduler)
                .ToSignal();

        private IObservable<Unit> OnResume() =>
            Observable
                .Start(() => this.ExecutionContext.IsPaused = false, this.scheduler)
                .ToSignal();

        private IObservable<Unit> OnSkipBackwards() =>
            this.SkipBackwards();

        private IObservable<Unit> OnSkipForwards() =>
            this.SkipForwards();

        private IObservable<Unit> Start(TimeSpan skipTo = default(TimeSpan), bool isPaused = false)
        {
            this.logger.Debug("Starting {0} from {1}.", isPaused ? "paused" : "unpaused", skipTo);

            var executionContext = new ExecutionContext(skipTo)
            {
                IsPaused = isPaused
            };

            return Observable
                .Using(
                    () => Disposable.Create(() => this.ExecutionContext = null),
                    _ =>
                        Observable
                            .Start(() => this.ExecutionContext = executionContext, this.scheduler)
                            .SelectMany(__ => this.model.Execute(executionContext)))
                .Catch<Unit, OperationCanceledException>(_ => Observables.Unit);
        }

        public IObservable<Unit> Stop()
        {
            this.logger.Debug("Stopping.");

            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                this.logger.Warn("Execution context is null - cannot stop.");
                return Observables.Unit;
            }

            executionContext.Cancel();

            return this
                .WhenAnyValue(x => x.IsStarted)
                .Where(x => !x)
                .ToSignal()
                .Do(_ => this.logger.Debug("Stop completed."))
                .FirstAsync();
        }

        private IObservable<Unit> SkipBackwards()
        {
            this.logger.Debug("Skipping backwards.");

            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                this.logger.Warn("Execution context is null - cannot skip backwards.");
                return Observables.Unit;
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
                .Stop()
                .Do(
                    _ =>
                    {
                        var skipTo = totalProgress -
                            currentExerciseProgress -
                            (currentExerciseProgress < skipBackwardsThreshold && priorExercise != null ? priorExercise.Duration : TimeSpan.Zero);

                        this
                            .Start(skipTo, isPaused)
                            .SubscribeSafe();
                        this.logger.Debug("Skip backwards completed.");
                    })
                .FirstAsync();
        }

        private IObservable<Unit> SkipForwards()
        {
            this.logger.Debug("Skipping forwards.");

            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                this.logger.Warn("Execution context is null - cannot skip forwards.");
                return Observables.Unit;
            }

            var totalProgress = executionContext.Progress;
            var currentExerciseProgress = executionContext.CurrentExerciseProgress;
            var currentExercise = executionContext.CurrentExercise;
            var isPaused = executionContext.IsPaused;

            return this
                .Stop()
                .Do(
                    _ =>
                    {
                        this
                            .Start(totalProgress - currentExerciseProgress + currentExercise.Duration, isPaused)
                            .SubscribeSafe();
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
                    this
                        .owner
                        .StartCommand
                        .Execute((TimeSpan?)parameter)
                        .SubscribeSafe();
                }
                else if (this.owner.IsPauseVisible)
                {
                    this
                        .owner
                        .PauseCommand
                        .Execute()
                        .SubscribeSafe();
                }
                else
                {
                    this
                        .owner
                        .ResumeCommand
                        .Execute()
                        .SubscribeSafe();
                }
            }
        }
    }
}