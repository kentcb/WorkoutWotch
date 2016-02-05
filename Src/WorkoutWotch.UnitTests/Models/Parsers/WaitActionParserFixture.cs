namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using PCLMock;
    using Services.Delay.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class WaitActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Theory]
        [InlineData("Wait for 1m 30s", 1 * msInMinute + 30 * msInSecond)]
        [InlineData("Wait for 1h 2m 5s", 1 * msInHour + 2 * msInMinute + 5 * msInSecond)]
        [InlineData("WAIT FOR 3s", 3 * msInSecond)]
        [InlineData("WaIt FoR 3s", 3 * msInSecond)]
        [InlineData("Wait    \t for \t   3s", 3 * msInSecond)]
        public void can_parse_correctly_formatted_input(string input, int expectedMilliseconds)
        {
            var result = WaitActionParser.GetParser(new DelayServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.NotNull(result);
            Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [Theory]
        [InlineData("  Wait for 1m")]
        [InlineData("Wait\n for 1m")]
        [InlineData("Wait for\n 1m")]
        [InlineData("Wayte for 1m")]
        [InlineData("Wait for abc")]
        [InlineData("WaitFor 1m")]
        [InlineData("Wait For1m")]
        [InlineData("WaitFor1m")]
        [InlineData("Wait 1m")]
        [InlineData("for 1m")]
        [InlineData("")]
        [InlineData("whatever")]
        public void cannot_parse_incorrectly_formatted_input(string input)
        {
            var result = WaitActionParser.GetParser(new DelayServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}