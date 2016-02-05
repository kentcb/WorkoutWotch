namespace WorkoutWotch.Models.Parsers
{
    using Sprache;
    using Utility;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class SayActionParser
    {
        public static Parser<SayAction> GetParser(ISpeechService speechService)
        {
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return
                from _ in Parse.IgnoreCase("say")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from speechText in StringLiteralParser.Parser
                select new SayAction(speechService, speechText);
        }
    }
}