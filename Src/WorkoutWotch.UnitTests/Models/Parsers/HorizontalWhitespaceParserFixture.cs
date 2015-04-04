namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class HorizontalWhitespaceParserFixture
    {
        [InlineData(" ")]
        [InlineData("\t")]
        public void can_parse_valid_input(string input)
        {
            var result = HorizontalWhitespaceParser.Parser(new Input(input));
            Assert.True(result.WasSuccessful && result.Remainder.AtEnd);
        }

        [InlineData("")]
        [InlineData("  ")]
        [InlineData("a")]
        [InlineData("\r")]
        [InlineData("\n")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = HorizontalWhitespaceParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}