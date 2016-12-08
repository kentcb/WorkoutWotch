namespace WorkoutWotch.Models
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using ReactiveUI;
    using Utility;

    public sealed class ExecutionContext : ReactiveObject
    {
        private readonly BehaviorSubject<bool> cancelRequested;
        private readonly Subject<TimeSpan> progressDeltas;
        private bool isCancelled;
        private TimeSpan progress;
        private TimeSpan currentExerciseProgress;
        private bool isPaused;
        private Exercise currentExercise;
        private int currentSet;
        private int currentRepetition;
        private TimeSpan skipAhead;

        public ExecutionContext(TimeSpan skipAhead = default(TimeSpan))
        {
            this.cancelRequested = new BehaviorSubject<bool>(false);
            this.progressDeltas = new Subject<TimeSpan>();

            this
                .cancelRequested
                .Where(x => x)
                .Subscribe(_ => this.IsCancelled = true);

            this
                .progressDeltas
                .Scan((running, next) => running + next)
                .Subscribe(x => this.Progress = x);

            this
                .WhenAnyValue(x => x.CurrentExercise)
                .Select(x => this.progressDeltas.StartWith(TimeSpan.Zero).Scan((running, next) => running + next))
                .Switch()
                .Subscribe(x => this.CurrentExerciseProgress = x);

            this
                .progressDeltas
                .StartWith(skipAhead)
                .Scan((running, next) => running - next)
                .Select(x => x < TimeSpan.Zero ? TimeSpan.Zero : x)
                .Subscribe(x => this.SkipAhead = x);
        }

        public bool IsCancelled
        {
            get { return this.isCancelled; }
            private set { this.RaiseAndSetIfChanged(ref this.isCancelled, value); }
        }

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

        public TimeSpan Progress
        {
            get { return this.progress; }
            private set { this.RaiseAndSetIfChanged(ref this.progress, value); }
        }

        public TimeSpan CurrentExerciseProgress
        {
            get { return this.currentExerciseProgress; }
            private set { this.RaiseAndSetIfChanged(ref this.currentExerciseProgress, value); }
        }

        public TimeSpan SkipAhead
        {
            get { return this.skipAhead; }
            private set { this.RaiseAndSetIfChanged(ref this.skipAhead, value); }
        }

        public IObservable<Unit> WaitWhilePaused() =>
            this.WhenAnyValue(x => x.IsPaused)
                .CombineLatest(this.cancelRequested, (isPaused, cancelRequested) => new { IsPaused = isPaused, CancelRequested = cancelRequested })
                .Where(x => !x.CancelRequested && !x.IsPaused)
                .FirstAsync()
                .Select(_ => Unit.Default);

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