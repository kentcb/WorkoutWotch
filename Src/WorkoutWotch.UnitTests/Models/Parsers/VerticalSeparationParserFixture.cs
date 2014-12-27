namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

    [TestFixture]
    public class VerticalSeparationParserFixture
    {
        [TestCase("")]
        [TestCase("\n")]
        [TestCase("\n\n\n\n\n\n")]
        [TestCase("\n\n\n\r\n\n\n\r\r\r\n\n\r\n")]
        [TestCase("\n  \t \n\n  \n\n  \t\n")]
        public void can_parse_valid_input(string input)
        {
            var result = VerticalSeparationParser.Parser(new Input(input));
            Assert.True(result.WasSuccessful && result.Remainder.AtEnd);
        }

        [TestCase("\na\n")]
        [TestCase("\n  ")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = VerticalSeparationParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}