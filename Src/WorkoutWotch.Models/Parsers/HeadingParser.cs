namespace WorkoutWotch.Models.Parsers
{
    using System.Linq;
    using Sprache;

    internal static class HeadingParser
    {
        public static Parser<string> GetParser(int level) => 
            from _ in Parse.String("#").Repeat(level)
            from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
            from name in Parse.AnyChar.Except(NewLineParser.Parser).AtLeastOnce()
            from ___ in NewLineParser.Parser.Or(ParseExt.Default<NewLineType>().End())
            let nameString = new string(name.ToArray()).TrimEnd()
            select nameString;
    }
}