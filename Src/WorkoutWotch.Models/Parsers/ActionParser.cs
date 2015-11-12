namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Logger;
    using Services.Contracts.Speech;
    using Sprache;

    internal static class ActionParser
    {
        public static Parser<IAction> GetParser(
            int indentLevel,
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            audioService.AssertNotNull(nameof(audioService));
            delayService.AssertNotNull(nameof(delayService));
            loggerService.AssertNotNull(nameof(loggerService));
            speechService.AssertNotNull(nameof(speechService));

            return BreakActionParser.GetParser(delayService, speechService)
                .Or<IAction>(MetronomeActionParser.GetParser(audioService, delayService, loggerService))
                .Or<IAction>(PrepareActionParser.GetParser(delayService, speechService))
                .Or<IAction>(SayActionParser.GetParser(speechService))
                .Or<IAction>(WaitActionParser.GetParser(delayService))
                .Or<IAction>(DoNotAwaitActionParser.GetParser(indentLevel, audioService, delayService, loggerService, speechService))
                .Or<IAction>(ParallelActionParser.GetParser(indentLevel, audioService, delayService, loggerService, speechService))
                .Or<IAction>(SequenceActionParser.GetParser(indentLevel, audioService, delayService, loggerService, speechService));
        }
    }
}