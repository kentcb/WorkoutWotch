namespace WorkoutWotch.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class ExerciseProgram
    {
        private readonly string name;
        private readonly IImmutableList<Exercise> exercises;
        private readonly TimeSpan duration;

        public ExerciseProgram(string name, IEnumerable<Exercise> exercises)
        {
            name.AssertNotNull("name");
            exercises.AssertNotNull("exercises", assertContentsNotNull: true);

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
                    // we can skip the exercise
                    context.AddProgress(exercise.Duration);
                    continue;
                }

                await exercise.ExecuteAsync(context).ContinueOnAnyContext();
            }
        }
    }
}