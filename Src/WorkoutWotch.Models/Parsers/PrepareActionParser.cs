namespace WorkoutWotch.Models.Parsers
{
    using Sprache;
    using Utility;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class PrepareActionParser
    {
        public static Parser<PrepareAction> GetParser(
            IDelayService delayService,
            ISpeechService speechService)
        {
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return
                from _ in Parse.IgnoreCase("prepare")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in Parse.IgnoreCase("for")
                from ____ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from duration in TimeSpanParser.Parser
                select new PrepareAction(
                    delayService,
                    speechService,
                    duration);
        }
    }
}