using System;
using Sprache;
using WorkoutWotch.Models.Actions;
using WorkoutWotch.Services.Contracts.Delay;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Parsers
{
    internal static class WaitActionParser
    {
        public static Parser<WaitAction> GetParser(IDelayService delayService)
        {
            delayService.AssertNotNull("delayService");

            return
                from _ in Parse.IgnoreCase("wait")
                from __ in Parse.WhiteSpace.AtLeastOnce()
                from ___ in Parse.IgnoreCase("for")
                from ____ in Parse.WhiteSpace.AtLeastOnce()
                from duration in TimeSpanParser.Parser
                select new WaitAction(delayService, duration);
        }
    }
}

