namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

    [TestFixture]
    public class HorizontalWhitespaceParserFixture
    {
        [TestCase(" ")]
        [TestCase("\t")]
        public void can_parse_valid_input(string input)
        {
            var result = HorizontalWhitespaceParser.Parser(new Input(input));
            Assert.True(result.WasSuccessful && result.Remainder.AtEnd);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase("a")]
        [TestCase("\r")]
        [TestCase("\n")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = HorizontalWhitespaceParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}