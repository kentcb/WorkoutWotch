namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class NewLineParserFixture
    {
        [Theory]
        [InlineData("\n", NewLineType.Posix)]
        [InlineData("\r\n", NewLineType.Windows)]
        [InlineData("\r", NewLineType.ClassicMac)]
        public void can_parse_new_lines(string input, int expected)
        {
            var result = NewLineParser.Parser.Parse(input);
            Assert.Equal((NewLineType)expected, result);
        }
    }
}