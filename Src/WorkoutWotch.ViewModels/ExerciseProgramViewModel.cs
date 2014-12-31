using System;
using System.Linq;
using ReactiveUI;
using WorkoutWotch.Utility;
using WorkoutWotch.Models;
using Kent.Boogaart.HelperTrinity.Extensions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WorkoutWotch.Services.Contracts.Scheduler;

namespace WorkoutWotch.ViewModels
{
	public sealed class ExerciseProgramViewModel : DisposableReactiveObject
	{
        // if an exercise has progressed less that this threshold and the user skips backwards, we will skip to the prior exercise
        // otherwise, we'll return to the start of the current exercise
        private static readonly TimeSpan skipBackwardsThreshold = TimeSpan.FromMilliseconds(500);

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
        private readonly ReactiveCommand<object> pauseCommand;
        private readonly ReactiveCommand<object> resumeCommand;
        private readonly IReactiveCommand skipBackwardsCommand;
        private readonly IReactiveCommand skipForwardsCommand;
        private ExecutionContext executionContext;

        public ExerciseProgramViewModel(ISchedulerService schedulerService, ExerciseProgram model)
        {
            schedulerService.AssertNotNull("schedulerService");
            model.AssertNotNull("model");

            this.model = model;
            this.disposables = new CompositeDisposable();
            this.exercises = this.model.Exercises.CreateDerivedCollection(x => new ExerciseViewModel(x));

            this.isStarted = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x != null)
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.IsStarted)
                .AddTo(this.disposables);

            this.isPaused = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(false) : x.WhenAnyValue(y => y.IsPaused))
                .Switch()
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.IsPaused)
                .AddTo(this.disposables);

            this.progressTimeSpan = this.WhenAnyValue(x => x.ExecutionContext)
                .Select(x => x == null ? Observable.Return(TimeSpan.Zero) : x.WhenAnyValue(y => y.Progress))
                .Switch()
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.ProgressTimeSpan)
                .AddTo(this.disposables);

            this.progress = this.WhenAnyValue(x => x.ProgressTimeSpan)
                .Select(x => x.TotalMilliseconds / this.model.Duration.TotalMilliseconds)
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.Progress)
                .AddTo(this.disposables);

            this.currentExercise = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ExecutionContext.CurrentExercise,
                    (ec, currentExercise) => ec == null ? null : currentExercise)
                .Select(x => this.Exercises.SingleOrDefault(y => y.Model == x))
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.CurrentExercise)
                .AddTo(this.disposables);

            var canStart = this.WhenAnyValue(x => x.IsStarted)
                .Select(x => !x);

            this.startCommand = ReactiveCommand
                .CreateAsyncTask(canStart, this.OnStartAsync, schedulerService.SynchronizationContextScheduler)
                .AddTo(this.disposables);

            var canPause = this.WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && !isPaused);

            this.pauseCommand = ReactiveCommand.Create(canPause, schedulerService.SynchronizationContextScheduler)
                .AddTo(this.disposables);

            var canResume = this.WhenAnyValue(x => x.IsStarted)
                .CombineLatest(this.WhenAnyValue(x => x.ExecutionContext.IsPaused), (isStarted, isPaused) => isStarted && isPaused);

            this.resumeCommand = ReactiveCommand.Create(canResume, schedulerService.SynchronizationContextScheduler)
                .AddTo(this.disposables);

            var canSkipBackwards = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.ProgressTimeSpan,
                    (ec, progress) => new { ExecutionContext = ec, Progress = progress })
                .Select(x => x.ExecutionContext != null && x.Progress >= skipBackwardsThreshold);

            this.skipBackwardsCommand = ReactiveCommand.CreateAsyncTask(canSkipBackwards, this.OnSkipBackwardsAsync, schedulerService.SynchronizationContextScheduler)
                .AddTo(this.disposables);

            var canSkipForwards = this.WhenAnyValue(
                    x => x.ExecutionContext,
                    x => x.CurrentExercise,
                    (ec, currentExercise) => new { ExecutionContext = ec, CurrentExercise = currentExercise })
                .Select(x => x.ExecutionContext != null && x.CurrentExercise != null && x.CurrentExercise != this.exercises.LastOrDefault());

            this.skipForwardsCommand = ReactiveCommand.CreateAsyncTask(canSkipForwards, this.OnSkipForwardsAsync, schedulerService.SynchronizationContextScheduler)
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

        public IReadOnlyReactiveList<ExerciseViewModel> Exercises
        {
            get { return this.exercises; }
        }

        public ExerciseViewModel CurrentExercise
        {
            get { return this.currentExercise.Value; }
        }

        public bool IsStarted
        {
            get { return this.isStarted.Value; }
        }

        public bool IsPaused
        {
            get { return this.isPaused.Value; }
        }

        public bool IsStartVisible
        {
            get { return this.isStartVisible.Value; }
        }

        public bool IsPauseVisible
        {
            get { return this.isPauseVisible.Value; }
        }

        public bool IsResumeVisible
        {
            get { return this.isResumeVisible.Value; }
        }

        public IReactiveCommand StartCommand
        {
            get { return this.startCommand; }
        }

        public IReactiveCommand PauseCommand
        {
            get { return this.pauseCommand; }
        }

        public IReactiveCommand ResumeCommand
        {
            get { return this.resumeCommand; }
        }

        public IReactiveCommand SkipBackwardsCommand
        {
            get { return this.skipBackwardsCommand; }
        }

        public IReactiveCommand SkipForwardsCommand
        {
            get { return this.skipForwardsCommand; }
        }

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

        private async Task OnStartAsync(object state)
        {
            await this.StartAsync(((TimeSpan?)state).GetValueOrDefault(TimeSpan.Zero));
        }

        private async Task OnSkipBackwardsAsync(object state)
        {
            await this.SkipBackwardsAsync();
        }

        private async Task OnSkipForwardsAsync(object state)
        {
            await this.SkipForwardsAsync();
        }

        private async Task StartAsync(TimeSpan skipTo = default(TimeSpan), bool isPaused = false)
        {
            var executionContext = new ExecutionContext(skipTo)
            {
                IsPaused = isPaused
            };

            using (executionContext)
            {
                this.ExecutionContext = executionContext;

                try
                {
                    await this.model.ExecuteAsync(executionContext);
                }
                catch (TaskCanceledException)
                {
                    // swallow
                }
            }

            this.ExecutionContext = null;
        }

        private async Task StopAsync()
        {
            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                return;
            }

            executionContext.Cancel();

            await this.WhenAnyValue(x => x.IsStarted)
                .Where(x => !x)
                .FirstAsync();
        }

        private async Task SkipBackwardsAsync()
        {
            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                return;
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

            await this.StopAsync();

            var skipTo = totalProgress -
                currentExerciseProgress -
                (currentExerciseProgress < skipBackwardsThreshold && priorExercise != null ? priorExercise.Duration : TimeSpan.Zero);

            this.StartAsync(skipTo, isPaused);
        }

        private async Task SkipForwardsAsync()
        {
            var executionContext = this.ExecutionContext;

            if (executionContext == null)
            {
                return;
            }

            var totalProgress = executionContext.Progress;
            var currentExerciseProgress = executionContext.CurrentExerciseProgress;
            var currentExercise = executionContext.CurrentExercise;
            var isPaused = executionContext.IsPaused;

            await this.StopAsync();

            this.StartAsync(totalProgress - currentExerciseProgress + currentExercise.Duration, isPaused);
        }

        private void OnThrownException(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
        }
	}
}

