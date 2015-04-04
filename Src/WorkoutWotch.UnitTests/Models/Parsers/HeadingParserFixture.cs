namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class HeadingParserFixture
    {
        [Theory]
        [InlineData("# Title", 1, "Title")]
        [InlineData("# Title\n", 1, "Title")]
        [InlineData("# title", 1, "title")]
        [InlineData("# title\n", 1, "title")]
        [InlineData("# A title with multiple words", 1, "A title with multiple words")]
        [InlineData("## Level two title", 2, "Level two title")]
        [InlineData("## Level two title\n", 2, "Level two title")]
        [InlineData("### Level three title", 3, "Level three title")]
        [InlineData("### Level three title\n", 3, "Level three title")]
        [InlineData("# \t  \t A title with surrounding whitespace  \t  \t\t ", 1, "A title with surrounding whitespace")]
        [InlineData("# A title    with \t  internal\twhitespace", 1, "A title    with \t  internal\twhitespace")]
        [InlineData("# A title cut short\nby new line", 1, "A title cut short")]
        public void can_parse_valid_input(string input, int level, string expected)
        {
            var result = HeadingParser.GetParser(level).Parse(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", 1)]
        [InlineData("#", 1)]
        [InlineData("#\n", 1)]
        [InlineData("# ", 1)]
        [InlineData("# \n", 1)]
        [InlineData("# A title at the wrong level", 2)]
        [InlineData("# A title at the wrong level\n", 2)]
        [InlineData("## Another title at the wrong level", 1)]
        [InlineData("## Another title at the wrong level\n", 1)]
        [InlineData("# A title cut short\nby new line", 1)]
        [InlineData(" # Leading whitespace is not allowed", 1)]
        public void cannot_parse_invalid_input(string input, int level)
        {
            var result = HeadingParser.GetParser(level)(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}