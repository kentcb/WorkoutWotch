namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using PCLMock;
    using Services.Delay.Mocks;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public sealed class PrepareActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Theory]
        [InlineData("Prepare for 24s", 24 * msInSecond)]
        [InlineData("Prepare for 1m 24s", 1 * msInMinute + 24 * msInSecond)]
        [InlineData("prepare For 24s", 24 * msInSecond)]
        [InlineData("Prepare   For \t 24s", 24 * msInSecond)]
        public void can_parse_valid_input(string input, int expectedMilliseconds)
        {
            var result = PrepareActionParser.GetParser(
                new DelayServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [Theory]
        [InlineData("  Prepare for 24s")]
        [InlineData("Prepare\n for 24s")]
        [InlineData("Prepare for\n 24s")]
        [InlineData("Preapre for 24s")]
        [InlineData("Preparefor 24s")]
        [InlineData("Prepare for")]
        [InlineData("Prepare for tea")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = PrepareActionParser.GetParser(
                new DelayServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}