using WorkoutWotch.Models.Parsers;
using WorkoutWotch.Services.Contracts.Audio;
using WorkoutWotch.Services.Contracts.Delay;
using WorkoutWotch.Services.Contracts.Logger;
using WorkoutWotch.Services.Contracts.Speech;
using Sprache;

namespace WorkoutWotch.Models
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Kent.Boogaart.HelperTrinity.Extensions;

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

        public static ExercisePrograms Parse(string input, IAudioService audioService, IDelayService delayService, ILoggerService loggerService, ISpeechService speechService)
        {
            input.AssertNotNull("input");
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");
            speechService.AssertNotNull("speechService");

            return ExerciseProgramsParser.GetParser(audioService, delayService, loggerService, speechService).Parse(input);
        }
    }
}