namespace WorkoutWotch.Models.Parsers
{
    using Sprache;
    using Utility;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class BreakActionParser
    {
        public static Parser<BreakAction> GetParser(
            IDelayService delayService,
            ISpeechService speechService)
        {
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return
                from _ in Parse.IgnoreCase("break")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in Parse.IgnoreCase("for")
                from ____ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from duration in TimeSpanParser.Parser
                select new BreakAction(
                    delayService,
                    speechService,
                    duration);
        }
    }
}