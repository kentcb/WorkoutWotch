namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Models.Actions;

    internal static class ParallelActionParser
    {
        public static Parser<ParallelAction> GetParser(
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
                from _ in Parse.IgnoreCase("parallel:")
                from __ in Parse.WhiteSpace.Except(NewLineParser.Parser).Many()
                from ___ in NewLineParser.Parser
                from actions in ActionListParser.GetParser(indentLevel + 1, audioService, delayService, loggerService, speechService)
                select new ParallelAction(actions);
        }
    }
}