namespace WorkoutWotch.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using Genesis.Logging;

    public sealed class ExerciseProgram
    {
        private readonly ILogger logger;
        private readonly string name;
        private readonly IImmutableList<Exercise> exercises;
        private readonly TimeSpan duration;

        public ExerciseProgram(string name, IEnumerable<Exercise> exercises)
        {
            Ensure.ArgumentNotNull(name, nameof(name));
            Ensure.ArgumentNotNull(exercises, nameof(exercises), assertContentsNotNull: true);

            this.logger = LoggerService.GetLogger(this.GetType());
            this.name = name;
            this.exercises = exercises.ToImmutableList();
            this.duration = this
                .exercises
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
        }

        public string Name => this.name;

        public TimeSpan Duration => this.duration;

        public IImmutableList<Exercise> Exercises => this.exercises;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            return Observable
                .Concat(
                    this
                        .exercises
                        .Select(
                            exercise =>
                            {
                                if (context.SkipAhead > TimeSpan.Zero && context.SkipAhead >= exercise.Duration)
                                {
                                    this.logger.Debug("Skipping exercise '{0}' because its duration ({1}) is less than the remaining skip ahead ({2}).", exercise.Name, exercise.Duration, context.SkipAhead);
                                    context.AddProgress(exercise.Duration);
                                    return Observable.Return(Unit.Default);
                                }

                                this.logger.Debug("Executing exercise '{0}'.", exercise.Name);
                                return exercise.Execute(context);
                            }))
                .DefaultIfEmpty();
        }
    }
}