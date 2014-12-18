namespace WorkoutWotch.Models
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.Services.Contracts.Container;

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

        public static ExercisePrograms Parse(string input, IContainerService containerService)
        {
            input.AssertNotNull("input");
            containerService.AssertNotNull("containerService");

            return ExerciseProgramsParser.GetParser(containerService).Parse(input);
        }
    }
}