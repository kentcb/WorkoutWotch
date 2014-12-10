namespace WorkoutWotch.Models.Parsers
{
    using Sprache;

    internal static class NewLineParser
    {
        public static readonly Parser<NewLineType> Parser = Parse.Char('\r').Then(_ => Parse.Char('\n')).Select(_ => NewLineType.Windows)
            .Or(Parse.Char('\n').Select(_ => NewLineType.Posix))
            .Or(Parse.Char('\r').Select(_ => NewLineType.ClassicMac));
    }
}