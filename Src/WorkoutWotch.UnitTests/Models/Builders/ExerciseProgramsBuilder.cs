namespace WorkoutWotch.UnitTests.Models.Builders
{
    using System.Collections.Generic;
    using WorkoutWotch.Models;

    internal sealed class ExerciseProgramsBuilder
    {
        private readonly IList<ExerciseProgram> programs;

        public ExerciseProgramsBuilder()
        {
            this.programs = new List<ExerciseProgram>();
        }

        public ExerciseProgramsBuilder AddProgram(ExerciseProgram program)
        {
            this.programs.Add(program);
            return this;
        }

        public ExerciseProgramsBuilder AddPrograms(IEnumerable<ExerciseProgram> programs)
        {
            foreach (var program in programs)
            {
                this.programs.Add(program);
            }

            return this;
        }

        public ExercisePrograms Build() =>
            new ExercisePrograms(this.programs);
        
        public static implicit operator ExercisePrograms(ExerciseProgramsBuilder builder) =>
            builder.Build();
    }
}