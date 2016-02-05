namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using PCLMock;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class SayActionParserFixture
    {
        [Theory]
        [InlineData("Say 'hello'", "hello")]
        [InlineData(@"Say ""hello""", "hello")]
        [InlineData("say 'hello'", "hello")]
        [InlineData("SAY 'hello'", "hello")]
        [InlineData("Say    \t \t   'hello'", "hello")]
        [InlineData(@"Say 'hello, how are you \'friend\'?'", "hello, how are you 'friend'?")]
        public void can_parse_correctly_formatted_input(string input, string expectedSpeechText)
        {
            var result = SayActionParser.GetParser(new SpeechServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.NotNull(result);
            Assert.Equal(expectedSpeechText, result.SpeechText);
        }

        [Theory]
        [InlineData("  Say 'hello'")]
        [InlineData("Say\n 'hello'")]
        [InlineData("Say")]
        [InlineData("Say hello")]
        [InlineData("Sai 'hello'")]
        [InlineData("Say'hello'")]
        public void cannot_parse_incorrectly_formatted_input(string input)
        {
            var result = SayActionParser.GetParser(new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}