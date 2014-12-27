namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Container;

    internal static class ExerciseProgramsParser
    {
        public static Parser<ExercisePrograms> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from _ in VerticalSeparationParser.Parser
                from exercisePrograms in ExerciseProgramParser.GetParser(containerService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                from __ in Parse.WhiteSpace.Many().End()
                select new ExercisePrograms(exercisePrograms.GetOrElse(Enumerable.Empty<ExerciseProgram>()));
        }
    }
}