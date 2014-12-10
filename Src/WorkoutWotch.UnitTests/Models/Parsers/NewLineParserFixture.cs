using System;
using NUnit.Framework;
using WorkoutWotch.Models.Parsers;
using Sprache;

namespace WorkoutWotch.UnitTests.Models.Parsers
{
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

