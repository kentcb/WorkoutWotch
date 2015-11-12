namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Speech;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Logger;

    internal static class ExerciseProgramParser
    {
        public static Parser<ExerciseProgram> GetParser(
                IAudioService audioService,
                IDelayService delayService,
                ILoggerService loggerService,
                ISpeechService speechService)
        {
            audioService.AssertNotNull(nameof(audioService));
            delayService.AssertNotNull(nameof(delayService));
            loggerService.AssertNotNull(nameof(loggerService));
            speechService.AssertNotNull(nameof(speechService));

            return
                from name in HeadingParser.GetParser(1)
                from _ in Parse.WhiteSpace.Many()
                from exercises in ExerciseParser.GetParser(audioService, delayService, loggerService, speechService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                select new ExerciseProgram(loggerService, name, exercises.GetOrElse(Enumerable.Empty<Exercise>()));
        }
    }
}