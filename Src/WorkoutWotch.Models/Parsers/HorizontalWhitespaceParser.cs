namespace WorkoutWotch.Models.Parsers
{
    using Sprache;

    internal static class HorizontalWhitespaceParser
    {
        public static Parser<char> Parser = Parse.WhiteSpace.Except(NewLineParser.Parser);
    }
}