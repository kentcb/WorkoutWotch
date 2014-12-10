namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Linq;
    using Sprache;

    internal static class TimeSpanParser
    {
        private static readonly Parser<int> hoursParser = Parse.Number.Then(x => Parse.IgnoreCase('h').Return(int.Parse(x)));

        private static readonly Parser<int> minutesParser = Parse.Number.Then(x => Parse.IgnoreCase('m').Return(int.Parse(x)));

        private static readonly Parser<double> secondsParser = Parse.Decimal.Then(x => Parse.IgnoreCase('s').Return(double.Parse(x)));

        public static readonly Parser<TimeSpan> Parser =
            from hours in hoursParser.Optional()
            from _ in (hours.IsDefined ? Parse.WhiteSpace.Except(NewLineParser.Parser).Many() : Parse.Return(Enumerable.Empty<char>()))
            from minutes in minutesParser.Optional()
            from __ in (minutes.IsDefined ? Parse.WhiteSpace.Except(NewLineParser.Parser).Many() : Parse.Return(Enumerable.Empty<char>()))
            from seconds in secondsParser.Optional()
            where hours.IsDefined || minutes.IsDefined || seconds.IsDefined
            select new TimeSpan(hours.GetOrDefault(), minutes.GetOrDefault(), 0) + TimeSpan.FromSeconds(seconds.GetOrDefault());
    }
}