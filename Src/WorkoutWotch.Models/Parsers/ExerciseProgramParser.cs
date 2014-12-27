namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Logger;

    internal static class ExerciseProgramParser
    {
        public static Parser<ExerciseProgram> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from name in HeadingParser.GetParser(1)
                from _ in Parse.WhiteSpace.Many()
                from exercises in ExerciseParser.GetParser(containerService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                select new ExerciseProgram(containerService.Resolve<ILoggerService>(), name, exercises.GetOrElse(Enumerable.Empty<Exercise>()));
        }
    }
}