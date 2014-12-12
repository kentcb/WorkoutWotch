using System;
using WorkoutWotch.Models.Actions;
using Sprache;
using WorkoutWotch.Services.Contracts.Audio;
using WorkoutWotch.Services.Contracts.Delay;
using Kent.Boogaart.HelperTrinity.Extensions;
using WorkoutWotch.Services.Contracts.Logger;

namespace WorkoutWotch.Models.Parsers
{
    internal static class MetronomeActionParser
    {
        private static Parser<MetronomeTick> metronomeTickParser =
            from periodBefore in TimeSpanParser.Parser
            from type in Parse.Chars("*-").Optional()
            select new MetronomeTick(periodBefore, GetTickType(type.GetOrDefault()));

        public static Parser<MetronomeAction> GetParser(IAudioService audioService, IDelayService delayService, ILoggerService loggerService)
        {
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");

            return
                from _ in Parse.IgnoreCase("metronome")
                from __ in Parse.WhiteSpace.AtLeastOnce()
                from ___ in Parse.IgnoreCase("at")
                from ____ in Parse.WhiteSpace.AtLeastOnce()
                from ticks in metronomeTickParser.DelimitedBy(Parse.Char(',').Token())
                select new MetronomeAction(audioService, delayService, loggerService, ticks);
        }

        private static MetronomeTickType GetTickType(char ch)
        {
            switch (ch)
            {
                case '*':
                    return MetronomeTickType.Bell;
                case '-':
                    return MetronomeTickType.None;
                default:
                    return MetronomeTickType.Click;
            }
        }
    }
}

