using System;
using Sprache;
using WorkoutWotch.Models.Actions;
using WorkoutWotch.Services.Contracts.Delay;
using WorkoutWotch.Services.Contracts.Speech;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Parsers
{
    internal static class BreakActionParser
    {
        public static Parser<BreakAction> GetParser(IDelayService delayService, ISpeechService speechService)
        {
            delayService.AssertNotNull("delayService");
            speechService.AssertNotNull("speechService");

            return
                from _ in Parse.IgnoreCase("break")
                from __ in Parse.WhiteSpace.AtLeastOnce()
                from ___ in Parse.IgnoreCase("for")
                from ____ in Parse.WhiteSpace.AtLeastOnce()
                from duration in TimeSpanParser.Parser
                select new BreakAction(delayService, speechService, duration);
        }
    }
}

