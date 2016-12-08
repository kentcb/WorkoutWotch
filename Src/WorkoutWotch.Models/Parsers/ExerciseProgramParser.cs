namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Genesis.Ensure;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Speech;
    using Sprache;

    internal static class ExerciseProgramParser
    {
        public static Parser<ExerciseProgram> GetParser(
                IAudioService audioService,
                IDelayService delayService,
                ISpeechService speechService)
        {
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return
                from name in HeadingParser.GetParser(1)
                from _ in Parse.WhiteSpace.Many()
                from exercises in ExerciseParser.GetParser(audioService, delayService, speechService).DelimitedBy(Parse.WhiteSpace.Many()).Optional()
                select new ExerciseProgram(name, exercises.GetOrElse(Enumerable.Empty<Exercise>()));
        }
    }
}