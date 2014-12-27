namespace WorkoutWotch.Models.Parsers
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class SayActionParser
    {
        public static Parser<SayAction> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from _ in Parse.IgnoreCase("say")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from speechText in StringLiteralParser.Parser
                select new SayAction(containerService.Resolve<ISpeechService>(), speechText);
        }
    }
}