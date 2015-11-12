namespace WorkoutWotch.Models.Parsers
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class SayActionParser
    {
        public static Parser<SayAction> GetParser(ISpeechService speechService)
        {
            speechService.AssertNotNull(nameof(speechService));

            return
                from _ in Parse.IgnoreCase("say")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from speechText in StringLiteralParser.Parser
                select new SayAction(speechService, speechText);
        }
    }
}