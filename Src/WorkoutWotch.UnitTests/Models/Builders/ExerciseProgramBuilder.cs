namespace WorkoutWotch.UnitTests.Models.Builders
{
    using System.Collections.Generic;
    using Genesis.TestUtil;
    using WorkoutWotch.Models;

    public sealed class ExerciseProgramBuilder : IBuilder
    {
        private List<Exercise> exercises;
        private string name;

        public ExerciseProgramBuilder()
        {
            this.exercises = new List<Exercise>();
            this.name = "Name";
        }

        public ExerciseProgramBuilder WithName(string name) =>
            this.With(ref this.name, name);

        public ExerciseProgramBuilder WithExercise(Exercise exercise) =>
            this.With(ref this.exercises, exercise);

        public ExerciseProgramBuilder WithExercises(IEnumerable<Exercise> exercises) =>
            this.With(ref this.exercises, exercises);

        public ExerciseProgram Build() =>
            new ExerciseProgram(
                this.name,
                this.exercises);

        public static implicit operator ExerciseProgram(ExerciseProgramBuilder builder) =>
            builder.Build();
    }
}