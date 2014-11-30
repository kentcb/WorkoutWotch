namespace WorkoutWotch.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class ExerciseProgram
    {
        private readonly ILogger logger;
        private readonly string name;
        private readonly IImmutableList<Exercise> exercises;
        private readonly TimeSpan duration;

        public ExerciseProgram(ILoggerService loggerService, string name, IEnumerable<Exercise> exercises)
        {
            loggerService.AssertNotNull("loggerService");
            name.AssertNotNull("name");
            exercises.AssertNotNull("exercises", assertContentsNotNull: true);

            this.logger = loggerService.GetLogger(this.GetType());
            this.name = name;
            this.exercises = exercises.ToImmutableList();
            this.duration = this
                .exercises
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
        }

        public string Name
        {
            get { return this.name; }
        }

        public TimeSpan Duration
        {
            get { return this.duration; }
        }

        public IImmutableList<Exercise> Exercises
        {
            get { return this.exercises; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");

            foreach (var exercise in this.exercises)
            {
                if (context.SkipAhead > TimeSpan.Zero && context.SkipAhead >= exercise.Duration)
                {
                    this.logger.Debug("Skipping exercise '{0}' because its duration ({1}) is less than the remaining skip ahead ({2}).", exercise.Name, exercise.Duration, context.SkipAhead);
                    context.AddProgress(exercise.Duration);
                    continue;
                }

                this.logger.Debug("Executing exercise '{0}'.", exercise.Name);
                await exercise.ExecuteAsync(context).ContinueOnAnyContext();
            }
        }
    }
}