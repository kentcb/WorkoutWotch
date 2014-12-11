namespace WorkoutWotch.Models.Parsers
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Models.Actions;

    internal static class SayActionParser
    {
        public static Parser<SayAction> GetParser(ISpeechService speechService)
        {
            speechService.AssertNotNull("speechService");

            return
                from _ in Parse.IgnoreCase("say")
                from __ in Parse.WhiteSpace.AtLeastOnce()
                from speechText in StringLiteralParser.Parser
                select new SayAction(speechService, speechText);
        }
    }
}