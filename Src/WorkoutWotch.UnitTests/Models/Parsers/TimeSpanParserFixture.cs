namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

    [TestFixture]
    public class TimeSpanParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [TestCase("3h", 3 * msInHour)]
        [TestCase("3m", 3 * msInMinute)]
        [TestCase("3s", 3 * msInSecond)]
        [TestCase("3h 4m", 3 * msInHour + 4 * msInMinute)]
        [TestCase("3h 4s", 3 * msInHour + 4 * msInSecond)]
        [TestCase("3h4s", 3 * msInHour + 4 * msInSecond)]
        [TestCase("4m 10s", 4 * msInMinute + 10 * msInSecond)]
        [TestCase("4m10s", 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3h 4m 10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3h4m10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3.5s", (int)(3.5 * msInSecond))]
        [TestCase("0.5s", (int)(0.5 * msInSecond))]
        [TestCase(".5s", (int)(0.5 * msInSecond))]
        public void can_parse_timespans(string input, int expectedMilliseconds)
        {
            var result = TimeSpanParser.Parser.Parse(input);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
        }

        [TestCase("3H", 3 * msInHour)]
        [TestCase("3M", 3 * msInMinute)]
        [TestCase("3S", 3 * msInSecond)]
        [TestCase("3h 4M", 3 * msInHour + 4 * msInMinute)]
        [TestCase("3h4M", 3 * msInHour + 4 * msInMinute)]
        [TestCase("3H 4s", 3 * msInHour + 4 * msInSecond)]
        [TestCase("4M 10S", 4 * msInMinute + 10 * msInSecond)]
        [TestCase("4M10S", 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3H 4m 10S", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3H 4M 10S", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3H4m10S", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        public void parsing_is_case_insensitive(string input, int expectedMilliseconds)
        {
            var result = TimeSpanParser.Parser.Parse(input);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
        }

        [TestCase("3h     4m       10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [TestCase("3h  \t   4m\t    \t  \t10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        public void whitespace_is_ignored(string input, int expectedMilliseconds)
        {
            var result = TimeSpanParser.Parser.Parse(input);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
        }

        [TestCase("abc")]
        [TestCase("31")]
        [TestCase("3.1h")]
        [TestCase("3.2m")]
        [TestCase("3h 10s 1m")]
        [TestCase("3h\r10s")]
        [TestCase("3h\n10s")]
        [TestCase("3h\r\n10s")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = TimeSpanParser.Parser(new Input(input));
            Assert.True(!result.WasSuccessful || !result.Remainder.AtEnd);
        }
    }
}

