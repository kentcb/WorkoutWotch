using System;
using Sprache;

namespace WorkoutWotch.Models.Parsers
{
    internal static class NewLineParser
    {
        public static readonly Parser<NewLineType> Parser = Parse.Char('\r').Then(_ => Parse.Char('\n')).Select(_ => NewLineType.Windows)
            .Or(Parse.Char('\n').Select(_ => NewLineType.Posix))
            .Or(Parse.Char('\r').Select(_ => NewLineType.ClassicMac));
    }
}

