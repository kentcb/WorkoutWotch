namespace WorkoutWotch.UnitTests.Models.Builders
{
    using System.Collections.Generic;
    using WorkoutWotch.Models;

    public sealed class ExerciseProgramsBuilder : IBuilder
    {
        private List<ExerciseProgram> programs;

        public ExerciseProgramsBuilder()
        {
            this.programs = new List<ExerciseProgram>();
        }

        public ExerciseProgramsBuilder WithProgram(ExerciseProgram program) =>
            this.With(ref this.programs, program);

        public ExerciseProgramsBuilder WithPrograms(IEnumerable<ExerciseProgram> programs) =>
            this.With(ref this.programs, programs);

        public ExercisePrograms Build() =>
            new ExercisePrograms(this.programs);

        public static implicit operator ExercisePrograms(ExerciseProgramsBuilder builder) =>
            builder.Build();
    }
}