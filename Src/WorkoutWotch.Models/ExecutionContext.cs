namespace WorkoutWotch.Models
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using ReactiveUI;

    public sealed class ExecutionContext : ReactiveObject
    {
        private readonly BehaviorSubject<bool> cancelRequested;
        private readonly Subject<TimeSpan> progressDeltas;
        private readonly ObservableAsPropertyHelper<bool> isCancelled;
        private readonly ObservableAsPropertyHelper<TimeSpan> currentExerciseProgress;
        private readonly ObservableAsPropertyHelper<TimeSpan> progress;
        private readonly ObservableAsPropertyHelper<TimeSpan> skipAhead;
        private bool isPaused;
        private Exercise currentExercise;
        private int currentSet;
        private int currentRepetition;

        public ExecutionContext(TimeSpan skipAhead = default(TimeSpan))
        {
            this.cancelRequested = new BehaviorSubject<bool>(false);
            this.progressDeltas = new Subject<TimeSpan>();

            this.isCancelled = this
                .cancelRequested
                .ToProperty(this, x => x.IsCancelled);

            this.progress = this
                .progressDeltas
                .Scan((running, next) => running + next)
                .ToProperty(this, x => x.Progress);

            this.currentExerciseProgress = this
                .WhenAnyValue(x => x.CurrentExercise)
                .Select(x => this.progressDeltas.StartWith(TimeSpan.Zero).Scan((running, next) => running + next))
                .Switch()
                .ToProperty(this, x => x.CurrentExerciseProgress);

            this.skipAhead = this
                .progressDeltas
                .StartWith(skipAhead)
                .Scan((running, next) => running - next)
                .Select(x => x < TimeSpan.Zero ? TimeSpan.Zero : x)
                // TODO: I don't understand why immediate scheduler is required here. It seems the heuristics in current thread scheduler are determining that scheduling
                //       is required rather than doing the work immediately.
                .ToProperty(this, x => x.SkipAhead, scheduler: ImmediateScheduler.Instance);
        }

        public bool IsCancelled => this.isCancelled.Value;

        public bool IsPaused
        {
            get { return this.isPaused; }
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

        public TimeSpan SkipAhead => this.skipAhead.Value;

        public IObservable<Unit> WaitWhilePaused() =>
            this.WhenAnyValue(x => x.IsPaused)
                .CombineLatest(this.cancelRequested, (isPaused, cancelRequested) => new { IsPaused = isPaused, CancelRequested = cancelRequested })
                .Where(x => !x.CancelRequested && !x.IsPaused)
                .FirstAsync()
                .ToSignal();

        public void Cancel() =>
            this.cancelRequested.OnNext(true);

        internal void AddProgress(TimeSpan progressDelta) =>
            this.progressDeltas.OnNext(progressDelta);

        internal void SetCurrentExercise(Exercise exercise) =>
            this.CurrentExercise = exercise;

        internal void SetCurrentSet(int set) =>
            this.CurrentSet = set;

        internal void SetCurrentRepetition(int repetition) =>
            this.CurrentRepetition = repetition;
    }
}