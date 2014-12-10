using System;
using NUnit.Framework;
using WorkoutWotch.Models.Parsers;
using Sprache;

namespace WorkoutWotch.UnitTests.Models.Parsers
{
    [TestFixture]
    public class StringLiteralParserFixture
    {
        [TestCase("''", "")]
        [TestCase("'  '", "  ")]
        [TestCase("'hello'", "hello")]
        [TestCase("'world'", "world")]
        [TestCase("'  hello, world!  '", "  hello, world!  ")]
        public void can_parse_single_quoted_strings(string input, string expected)
        {
            var result = StringLiteralParser.Parser.Parse(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase(@"""""", "")]
        [TestCase(@"""  """, "  ")]
        [TestCase(@"""hello""", "hello")]
        [TestCase(@"""world""", "world")]
        [TestCase(@"""  hello, world!  """, "  hello, world!  ")]
        public void can_parse_double_quoted_strings(string input, string expected)
        {
            var result = StringLiteralParser.Parser.Parse(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase(@"'hello \'friend\''", "hello 'friend'")]
        [TestCase(@"""hello \""friend\""""", @"hello ""friend""")]
        public void can_parse_escaped_strings(string input, string expected)
        {
            var result = StringLiteralParser.Parser.Parse(input);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void cannot_parse_empty_input()
        {
            var result = StringLiteralParser.Parser(new Input(""));
            Assert.False(result.WasSuccessful);
            Assert.AreEqual("unexpected end of input", result.Message);
        }

        [TestCase("  ")]
        [TestCase("hello")]
        [TestCase("world")]
        public void cannot_parse_non_delimitered_string(string input)
        {
            var result = StringLiteralParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful);
            Assert.AreEqual("unexpected '" + input[0] + "'", result.Message);
        }

        [TestCase("'")]
        [TestCase(@"""")]
        [TestCase("'hello")]
        [TestCase(@"""hello")]
        public void cannot_parse_non_terminated_string(string input)
        {
            var result = StringLiteralParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful);
            Assert.AreEqual("unexpected end of input", result.Message);
        }
    }
}

