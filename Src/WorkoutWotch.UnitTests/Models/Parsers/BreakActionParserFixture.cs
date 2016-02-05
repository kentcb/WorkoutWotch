namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using PCLMock;
    using Services.Delay.Mocks;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class BreakActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Fact]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BreakActionParser.GetParser(null, new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BreakActionParser.GetParser(new DelayServiceMock(), null));
        }

        [Theory]
        [InlineData("Break for 24s", 24 * msInSecond)]
        [InlineData("Break for 1m 24s", 1 * msInMinute + 24 * msInSecond)]
        [InlineData("break For 24s", 24 * msInSecond)]
        [InlineData("Break   For \t 24s", 24 * msInSecond)]
        public void can_parse_valid_input(string input, int expectedMilliseconds)
        {
            var result = BreakActionParser.GetParser(
                new DelayServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [Theory]
        [InlineData("  Break for 24s")]
        [InlineData("Brake for 24s")]
        [InlineData("Breakfor 24s")]
        [InlineData("Break for24s")]
        [InlineData("Break for")]
        [InlineData("Break for tea")]
        [InlineData("Break\nfor 24s")]
        [InlineData("Break for\n24s")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = BreakActionParser.GetParser(
               new DelayServiceMock(MockBehavior.Loose),
               new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}