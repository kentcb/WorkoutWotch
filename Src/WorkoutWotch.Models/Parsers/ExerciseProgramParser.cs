namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Logger;

    internal static class ExerciseProgramParser
    {
        private static readonly Parser<string> nameParser =
            from _ in Parse.String("#")
            from __ in Parse.WhiteSpace.Except(NewLineParser.Parser).AtLeastOnce()
            from name in Parse.AnyChar.Except(NewLineParser.Parser).AtLeastOnce()
            from ___ in NewLineParser.Parser
            let nameString = new string(name.ToArray()).TrimEnd()
            select nameString;

        public static Parser<ExerciseProgram> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from name in nameParser
                from exercises in ExerciseParser.GetParser(containerService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                select new ExerciseProgram(containerService.Resolve<ILoggerService>(), name, exercises.GetOrElse(Enumerable.Empty<Exercise>()));
        }
    }
}