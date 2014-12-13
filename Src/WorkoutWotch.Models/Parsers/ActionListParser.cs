namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Collections.Generic;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;

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

            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");
            speechService.AssertNotNull("speechService");

            return
                (from _ in Parse.String(new string(' ', indentLevel * 2))
                 from __ in Parse.String("* ")
                 from action in ActionParser.GetParser(indentLevel, audioService, delayService, loggerService, speechService)
                 from ___ in Parse.WhiteSpace.Except(NewLineParser.Parser).Many()
                 select action).DelimitedBy(NewLineParser.Parser);
        }
    }
}