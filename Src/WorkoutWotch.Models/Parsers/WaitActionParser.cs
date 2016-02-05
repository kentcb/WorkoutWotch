namespace WorkoutWotch.Models.Parsers
{
    using Sprache;
    using Utility;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;

    internal static class WaitActionParser
    {
        public static Parser<WaitAction> GetParser(IDelayService delayService)
        {
            Ensure.ArgumentNotNull(delayService, nameof(delayService));

            return
                from _ in Parse.IgnoreCase("wait")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in Parse.IgnoreCase("for")
                from ____ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from duration in TimeSpanParser.Parser
                select new WaitAction(delayService, duration);
        }
    }
}