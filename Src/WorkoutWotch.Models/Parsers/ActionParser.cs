namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Logger;
    using Services.Contracts.Speech;
    using Sprache;
    using Utility;

    internal static class ActionParser
    {
        public static Parser<IAction> GetParser(
            int indentLevel,
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            Ensure.ArgumentCondition(indentLevel >= 0, "indentLevel must be greater than or equal to 0.", "indentLevel");
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

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