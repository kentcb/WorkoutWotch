namespace WorkoutWotch.Models
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Genesis.Ensure;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Logger;
    using Services.Contracts.Speech;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

    public sealed class ExercisePrograms
    {
        private readonly IImmutableList<ExerciseProgram> programs;

        public ExercisePrograms(IEnumerable<ExerciseProgram> programs)
        {
            Ensure.ArgumentNotNull(programs, nameof(programs), assertContentsNotNull: true);
            this.programs = programs.ToImmutableList();
        }

        public IImmutableList<ExerciseProgram> Programs => this.programs;

        public static ExercisePrograms Parse(
            string input,
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            Ensure.ArgumentNotNull(input, nameof(input));
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return ExerciseProgramsParser.GetParser(audioService, delayService, loggerService, speechService).Parse(input);
        }

        public static IResult<ExercisePrograms> TryParse(
            string input,
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            Ensure.ArgumentNotNull(input, nameof(input));
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return ExerciseProgramsParser.GetParser(audioService, delayService, loggerService, speechService).TryParse(input);
        }
    }
}