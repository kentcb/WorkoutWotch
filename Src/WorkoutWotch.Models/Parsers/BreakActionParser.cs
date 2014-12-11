namespace WorkoutWotch.Models.Parsers
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Models.Actions;

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