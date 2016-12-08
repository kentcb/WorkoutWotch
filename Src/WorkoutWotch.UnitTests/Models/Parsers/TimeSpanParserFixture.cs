namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public sealed class TimeSpanParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Theory]
        [InlineData("3h", 3 * msInHour)]
        [InlineData("3m", 3 * msInMinute)]
        [InlineData("3s", 3 * msInSecond)]
        [InlineData("3h 4m", 3 * msInHour + 4 * msInMinute)]
        [InlineData("3h 4s", 3 * msInHour + 4 * msInSecond)]
        [InlineData("3h4s", 3 * msInHour + 4 * msInSecond)]
        [InlineData("4m 10s", 4 * msInMinute + 10 * msInSecond)]
        [InlineData("4m10s", 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3h 4m 10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3h4m10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3.5s", (int)(3.5 * msInSecond))]
        [InlineData("0.5s", (int)(0.5 * msInSecond))]
        [InlineData(".5s", (int)(0.5 * msInSecond))]
        [InlineData("3H", 3 * msInHour)]
        [InlineData("3M", 3 * msInMinute)]
        [InlineData("3S", 3 * msInSecond)]
        [InlineData("3h 4M", 3 * msInHour + 4 * msInMinute)]
        [InlineData("3h4M", 3 * msInHour + 4 * msInMinute)]
        [InlineData("3H 4s", 3 * msInHour + 4 * msInSecond)]
        [InlineData("4M 10S", 4 * msInMinute + 10 * msInSecond)]
        [InlineData("4M10S", 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3H 4m 10S", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3H 4M 10S", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3H4m10S", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3h     4m       10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        [InlineData("3h  \t   4m\t    \t  \t10s", 3 * msInHour + 4 * msInMinute + 10 * msInSecond)]
        public void can_parse_timespans(string input, int expectedMilliseconds)
        {
            var result = TimeSpanParser.Parser.Parse(input);
            Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("31")]
        [InlineData("3.1h")]
        [InlineData("3.2m")]
        [InlineData("3h 10s 1m")]
        [InlineData("3h\r10s")]
        [InlineData("3h\n10s")]
        [InlineData("3h\r\n10s")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = TimeSpanParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}

