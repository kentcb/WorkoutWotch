namespace WorkoutWotch.Models.Parsers
{
    using System.Collections.Generic;
    using Genesis.Ensure;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Logger;
    using Services.Contracts.Speech;
    using Sprache;

    internal static class ActionListParser
    {
        public static Parser<IEnumerable<IAction>> GetParser(
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

            return
                (from _ in Parse.String("  ").Or(Parse.String("\t")).Repeat(indentLevel)
                 from __ in Parse.String("* ")
                 from action in ActionParser.GetParser(indentLevel, audioService, delayService, loggerService, speechService).Token(HorizontalWhitespaceParser.Parser)
                 select action).DelimitedBy(NewLineParser.Parser);
        }
    }
}