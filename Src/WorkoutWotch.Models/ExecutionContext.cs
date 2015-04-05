namespace WorkoutWotch.Models
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using ReactiveUI;
    using WorkoutWotch.Utility;

    public sealed class ExecutionContext : DisposableReactiveObject
    {
        private readonly CompositeDisposable disposables;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ObservableAsPropertyHelper<bool> isCancelled;
        private readonly ObservableAsPropertyHelper<TimeSpan> progress;
        private readonly ObservableAsPropertyHelper<TimeSpan> currentExerciseProgress;
        private readonly Subject<Unit> cancelRequested;
        private readonly Subject<TimeSpan> progressDeltas;
        private bool isPaused;
        private Exercise currentExercise;
        private int currentSet;
        private int currentRepetition;
        private TimeSpan skipAhead;

        public ExecutionContext(TimeSpan skipAhead = default(TimeSpan))
        {
            this.disposables = new CompositeDisposable();
            this.cancellationTokenSource = new CancellationTokenSource()
                .AddTo(this.disposables);
            this.cancelRequested = new Subject<Unit>()
                .AddTo(this.disposables);
            this.progressDeltas = new Subject<TimeSpan>()
                .AddTo(this.disposables);

            this.isCancelled = this.cancelRequested
                .Select(_ => true)
                .ToProperty(this, x => x.IsCancelled)
                .AddTo(this.disposables);

            this.WhenAnyValue(x => x.IsCancelled)
                .Where(x => x)
                .Subscribe(_ => this.cancellationTokenSource.Cancel())
                .AddTo(this.disposables);

            this.progress = this.progressDeltas
                .Scan((running, next) => running + next)
                .ToProperty(this, x => x.Progress)
                .AddTo(this.disposables);

            this.currentExerciseProgress = this.WhenAnyValue(x => x.CurrentExercise)
                .Select(x => this.progressDeltas.StartWith(TimeSpan.Zero).Scan((running, next) => running + next))
                .Switch()
                .ToProperty(this, x => x.CurrentExerciseProgress)
                .AddTo(this.disposables);

            // cannot use ToProperty without also "hacking in" the immediate scheduler - see https://github.com/reactiveui/ReactiveUI/issues/785
            this.progressDeltas
                .StartWith(skipAhead)
                .Scan((running, next) => running - next)
                .Select(x => x < TimeSpan.Zero ? TimeSpan.Zero : x)
                .Subscribe(x => this.SkipAhead = x)
                .AddTo(this.disposables);
        }

        public CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        public bool IsCancelled => this.isCancelled.Value;

        public bool IsPaused
        {
            get {return this.isPaused; }
            set { this.RaiseAndSetIfChanged(ref this.isPaused, value); }
        }

        public Exercise CurrentExercise
        {
            get { return this.currentExercise; }
            private set { this.RaiseAndSetIfChanged(ref this.currentExercise, value); }
        }

        public int CurrentSet
        {
            get { return this.currentSet; }
            private set { this.RaiseAndSetIfChanged(ref this.currentSet, value); }
        }

        public int CurrentRepetition
        {
            get { return this.currentRepetition; }
            private set { this.RaiseAndSetIfChanged(ref this.currentRepetition, value); }
        }

        public TimeSpan Progress => this.progress.Value;

        public TimeSpan CurrentExerciseProgress => this.currentExerciseProgress.Value;

        public TimeSpan SkipAhead
        {
            get { return this.skipAhead; }
            private set { this.RaiseAndSetIfChanged(ref this.skipAhead, value); }
        }

        public Task WaitWhilePausedAsync()
        {
            return this.WhenAnyValue(x => x.IsPaused)
                .Where(x => !x)
                .FirstAsync()
                .ToTask(this.CancellationToken);
        }

        public void Cancel()
            =>  this.cancelRequested.OnNext(Unit.Default);

        internal void AddProgress(TimeSpan progressDelta)
            =>  this.progressDeltas.OnNext(progressDelta);

        internal void SetCurrentExercise(Exercise exercise)
            => this.CurrentExercise = exercise;

        internal void SetCurrentSet(int set)
            => this.CurrentSet = set;

        internal void SetCurrentRepetition(int repetition)
            => this.CurrentRepetition = repetition;

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