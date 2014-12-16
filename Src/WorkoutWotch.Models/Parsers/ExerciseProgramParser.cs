using System;
using Sprache;
using WorkoutWotch.Services.Contracts.Audio;
using WorkoutWotch.Services.Contracts.Delay;
using WorkoutWotch.Services.Contracts.Logger;
using WorkoutWotch.Services.Contracts.Speech;
using System.Linq;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Parsers
{
    internal static class ExerciseProgramParser
    {
        private static readonly Parser<string> nameParser =
            from _ in Parse.String("#")
            from name in Parse.AnyChar.Until(NewLineParser.Parser)
            select new string(name.ToArray()).Trim();

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

