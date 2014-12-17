using System;
using Sprache;
using WorkoutWotch.Services.Contracts.Audio;
using WorkoutWotch.Services.Contracts.Delay;
using WorkoutWotch.Services.Contracts.Logger;
using WorkoutWotch.Services.Contracts.Speech;
using Kent.Boogaart.HelperTrinity.Extensions;
using System.Linq;

namespace WorkoutWotch.Models.Parsers
{
    internal static class ExerciseProgramsParser
    {
        public static Parser<ExercisePrograms> GetParser(IAudioService audioService, IDelayService delayService, ILoggerService loggerService, ISpeechService speechService)
        {
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");
            speechService.AssertNotNull("speechService");

            return
                from exercisePrograms in ExerciseProgramParser.GetParser(audioService, delayService, loggerService, speechService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                from _ in Parse.WhiteSpace.Many().End()
                select new ExercisePrograms(exercisePrograms.GetOrElse(Enumerable.Empty<ExerciseProgram>()));
        }

    }
}

