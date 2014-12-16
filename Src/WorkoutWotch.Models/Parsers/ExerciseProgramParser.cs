namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class ExerciseProgramParser
    {
        private static readonly Parser<string> nameParser =
            from _ in Parse.String("#")
            from __ in Parse.WhiteSpace.Except(NewLineParser.Parser).AtLeastOnce()
            from name in Parse.AnyChar.Except(NewLineParser.Parser).AtLeastOnce()
            from ___ in NewLineParser.Parser
            let nameString = new string(name.ToArray()).TrimEnd()
            select nameString;

        public static Parser<ExerciseProgram> GetParser(IAudioService audioService, IDelayService delayService, ILoggerService loggerService, ISpeechService speechService)
        {
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");
            speechService.AssertNotNull("speechService");

            return
                from name in nameParser
                from exercises in ExerciseParser.GetParser(audioService, delayService, loggerService, speechService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                select new ExerciseProgram(loggerService, name, exercises.GetOrElse(Enumerable.Empty<Exercise>()));
        }
    }
}