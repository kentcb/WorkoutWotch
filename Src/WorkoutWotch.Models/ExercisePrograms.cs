namespace WorkoutWotch.Models
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class ExercisePrograms
    {
        private readonly IImmutableList<ExerciseProgram> programs;

        public ExercisePrograms(IEnumerable<ExerciseProgram> programs)
        {
            programs.AssertNotNull("programs", assertContentsNotNull: true);
            this.programs = programs.ToImmutableList();
        }

        public IImmutableList<ExerciseProgram> Programs
        {
            get { return this.programs; }
        }
    }
}