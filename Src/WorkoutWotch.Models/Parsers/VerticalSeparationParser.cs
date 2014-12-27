namespace WorkoutWotch.Models.Parsers
{
    using System.Reactive;
    using Sprache;

    internal static class VerticalSeparationParser
    {
        // any amount of horizontal whitespace and new lines as long as it all ends with a new line
        public static Parser<Unit> Parser = HorizontalWhitespaceParser.Parser
            .Many()
            .Then(x => NewLineParser.Parser)
            .Many()
            .ToUnit();
    }
}