namespace WorkoutWotch.UnitTests.Models.Builders
{
    using System.Collections.Generic;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    public sealed class ExerciseProgramBuilder : IBuilder
    {
        private List<Exercise> exercises;
        private ILoggerService loggerService;
        private string name;

        public ExerciseProgramBuilder()
        {
            this.exercises = new List<Exercise>();
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.name = "Name";
        }

        public ExerciseProgramBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public ExerciseProgramBuilder WithName(string name) =>
            this.With(ref this.name, name);

        public ExerciseProgramBuilder WithExercise(Exercise exercise) =>
            this.With(ref this.exercises, exercise);

        public ExerciseProgramBuilder WithExercises(IEnumerable<Exercise> exercises) =>
            this.With(ref this.exercises, exercises);

        public ExerciseProgram Build() =>
            new ExerciseProgram(
                this.loggerService,
                this.name,
                this.exercises);

        public static implicit operator ExerciseProgram(ExerciseProgramBuilder builder) =>
            builder.Build();
    }
}