namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;

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
        [TestCase(@"'foo\\bar'", @"foo\bar")]
        [TestCase(@"""foo\\bar""", @"foo\bar")]
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
            Assert.AreEqual(2, result.Expectations.Count());
            Assert.AreEqual("string delimited by \"", result.Expectations.ElementAt(0));
            Assert.AreEqual("string delimited by '", result.Expectations.ElementAt(1));
        }

        [TestCase("  ")]
        [TestCase("hello")]
        [TestCase("world")]
        public void cannot_parse_non_delimitered_string(string input)
        {
            var result = StringLiteralParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful);
            Assert.AreEqual("unexpected '" + input[0] + "'", result.Message);
            Assert.AreEqual(2, result.Expectations.Count());
            Assert.AreEqual("\"", result.Expectations.ElementAt(0));
            Assert.AreEqual("'", result.Expectations.ElementAt(1));
        }

        [TestCase('\'', "")]
        [TestCase('"', "")]
        [TestCase('\'', "hello")]
        [TestCase('"', "hello")]
        public void cannot_parse_non_terminated_string(char delimiter, string input)
        {
            var result = StringLiteralParser.Parser(new Input(delimiter + input));
            Assert.False(result.WasSuccessful);
            Assert.AreEqual("unexpected end of input", result.Message);
            Assert.AreEqual(1, result.Expectations.Count());
            Assert.AreEqual("continued string contents or " + delimiter, result.Expectations.ElementAt(0));
       }
    }
}