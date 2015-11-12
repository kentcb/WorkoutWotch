namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Collections.Generic;
    using Kent.Boogaart.HelperTrinity.Extensions;
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
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            audioService.AssertNotNull(nameof(audioService));
            delayService.AssertNotNull(nameof(delayService));
            loggerService.AssertNotNull(nameof(loggerService));
            speechService.AssertNotNull(nameof(speechService));

            return
                (from _ in Parse.String("  ").Or(Parse.String("\t")).Repeat(indentLevel)
                 from __ in Parse.String("* ")
                 from action in ActionParser.GetParser(indentLevel, audioService, delayService, loggerService, speechService).Token(HorizontalWhitespaceParser.Parser)
                 select action).DelimitedBy(NewLineParser.Parser);
        }
    }
}