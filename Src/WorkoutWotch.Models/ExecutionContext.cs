using WorkoutWotch.Utility;
using System;
using System.Threading;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Disposables;

namespace WorkoutWotch.Models
{
	public sealed class ExecutionContext : DisposableReactiveObject
	{
        private readonly CompositeDisposable disposables;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Subject<Unit> cancelRequested;
        private readonly ObservableAsPropertyHelper<bool> isCancelled;
        private readonly Subject<TimeSpan> progressDeltas;
        private readonly ObservableAsPropertyHelper<TimeSpan> progress;
        private readonly ObservableAsPropertyHelper<TimeSpan> currentExerciseProgress;
        private readonly ObservableAsPropertyHelper<TimeSpan> skipAhead;
        private bool isPaused;
        private Exercise currentExercise;
        private int currentSet;
        private int currentRepetition;

        public ExecutionContext(TimeSpan skipAhead = default(TimeSpan))
        {
            this.disposables = new CompositeDisposable();
            this.cancellationTokenSource = new CancellationTokenSource().AddTo(this.disposables);
            this.cancelRequested = new Subject<Unit>();
            this.isCancelled = this.cancelRequested
                .Select(_ => true)
                .ToProperty(this, x => x.IsCancelled)
                .AddTo(this.disposables);
            this.WhenAnyValue(x => x.IsCancelled)
                .Where(x => x)
                .Subscribe(_ => this.cancellationTokenSource.Cancel())
                .AddTo(this.disposables);
            this.progressDeltas = new Subject<TimeSpan>().AddTo(this.disposables);
            this.progress = this.progressDeltas
                .Scan((running, next) => running + next)
                .ToProperty(this, x => x.Progress)
                .AddTo(this.disposables);
            this.currentExerciseProgress = this.WhenAnyValue(x => x.CurrentExercise)
                .Select(x => this.progressDeltas.StartWith(TimeSpan.Zero).Scan((running, next) => running + next))
                .Switch()
                .ToProperty(this, x => x.CurrentExerciseProgress)
                .AddTo(this.disposables);
            this.skipAhead = this.progressDeltas
                .StartWith(skipAhead)
                .Scan((running, next) => running - next)
                .Select(x => x < TimeSpan.Zero ? TimeSpan.Zero : x)
                .ToProperty(this, x => x.SkipAhead)
                .AddTo(this.disposables);
        }

        public CancellationToken CancellationToken
        {
            get{ return this.cancellationTokenSource.Token; }
        }

        public bool IsCancelled
        {
            get { return this.isCancelled.Value; }
        }

        public bool IsPaused
        {
            get {return this.isPaused; }
            set{ this.RaiseAndSetIfChanged(ref this.isPaused, value); }
        }

        public Exercise CurrentExercise
        {
            get{ return this.currentExercise; }
            private set { this.RaiseAndSetIfChanged(ref this.currentExercise, value); }
        }

        public int CurrentSet
        {
            get{ return this.currentSet; }
            private set { this.RaiseAndSetIfChanged(ref this.currentSet, value); }
        }

        public int CurrentRepetition
        {
            get{ return this.currentRepetition; }
            private set { this.RaiseAndSetIfChanged(ref this.currentRepetition, value); }
        }

        public TimeSpan Progress
        {
            get{ return this.progress.Value; }
        }

        public TimeSpan CurrentExerciseProgress
        {
            get{ return this.currentExerciseProgress.Value; }
        }

        public TimeSpan SkipAhead
        {
            get{ return this.skipAhead.Value; }
        }

        public Task WaitWhilePausedAsync()
        {
            return this.WhenAnyValue(x => x.IsPaused)
                .Where(x => !x)
                .FirstAsync()
                .ToTask(this.CancellationToken);
        }

        public void Cancel()
        {
            this.cancelRequested.OnNext(Unit.Default);
        }

        internal void AddProgress(TimeSpan progressDelta)
        {
            this.progressDeltas.OnNext(progressDelta);
        }

        internal void SetCurrentExercise(Exercise exercise)
        {
            this.CurrentExercise = exercise;
        }

        internal void SetCurrentSet(int set)
        {
            this.CurrentSet = set;
        }

        internal void SetCurrentRepetition(int repetition)
        {
            this.CurrentRepetition = repetition;
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