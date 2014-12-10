namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

    [TestFixture]
    public class NewLineParserFixture
    {
        [TestCase("\n", NewLineType.Posix)]
        [TestCase("\r\n", NewLineType.Windows)]
        [TestCase("\r", NewLineType.ClassicMac)]
        public void can_parse_new_lines(string input, int expected)
        {
            var result = NewLineParser.Parser.Parse(input);
            Assert.AreEqual((NewLineType)expected, result);
        }
    }
}