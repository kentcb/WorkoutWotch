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

        public ExercisePrograms Build()
        {
            return new ExercisePrograms(this.programs);
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
    }
}