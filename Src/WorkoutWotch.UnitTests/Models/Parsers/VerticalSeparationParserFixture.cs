namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class VerticalSeparationParserFixture
    {
        [Theory]
        [InlineData("")]
        [InlineData("\n")]
        [InlineData("\n\n\n\n\n\n")]
        [InlineData("\n\n\n\r\n\n\n\r\r\r\n\n\r\n")]
        [InlineData("\n  \t \n\n  \n\n  \t\n")]
        public void can_parse_valid_input(string input)
        {
            var result = VerticalSeparationParser.Parser(new Input(input));
            Assert.True(result.WasSuccessful && result.Remainder.AtEnd);
        }

        [Theory]
        [InlineData("\na\n")]
        [InlineData("\n  ")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = VerticalSeparationParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}