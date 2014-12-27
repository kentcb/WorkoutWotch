namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

    [TestFixture]
    public class HeadingParserFixture
    {
        [TestCase("# Title", 1, "Title")]
        [TestCase("# Title\n", 1, "Title")]
        [TestCase("# title", 1, "title")]
        [TestCase("# title\n", 1, "title")]
        [TestCase("# A title with multiple words", 1, "A title with multiple words")]
        [TestCase("## Level two title", 2, "Level two title")]
        [TestCase("## Level two title\n", 2, "Level two title")]
        [TestCase("### Level three title", 3, "Level three title")]
        [TestCase("### Level three title\n", 3, "Level three title")]
        [TestCase("# \t  \t A title with surrounding whitespace  \t  \t\t ", 1, "A title with surrounding whitespace")]
        [TestCase("# A title    with \t  internal\twhitespace", 1, "A title    with \t  internal\twhitespace")]
        [TestCase("# A title cut short\nby new line", 1, "A title cut short")]
        public void can_parse_valid_input(string input, int level, string expected)
        {
            var result = HeadingParser.GetParser(level).Parse(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase("", 1)]
        [TestCase("#", 1)]
        [TestCase("#\n", 1)]
        [TestCase("# ", 1)]
        [TestCase("# \n", 1)]
        [TestCase("# A title at the wrong level", 2)]
        [TestCase("# A title at the wrong level\n", 2)]
        [TestCase("## Another title at the wrong level", 1)]
        [TestCase("## Another title at the wrong level\n", 1)]
        [TestCase("# A title cut short\nby new line", 1)]
        [TestCase(" # Leading whitespace is not allowed", 1)]
        public void cannot_parse_invalid_input(string input, int level)
        {
            var result = HeadingParser.GetParser(level)(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}