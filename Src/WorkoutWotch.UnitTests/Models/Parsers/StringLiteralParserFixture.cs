namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public sealed class StringLiteralParserFixture
    {
        [Theory]
        [InlineData("''", "")]
        [InlineData("'  '", "  ")]
        [InlineData("'hello'", "hello")]
        [InlineData("'world'", "world")]
        [InlineData("'  hello, world!  '", "  hello, world!  ")]
        public void can_parse_single_quoted_strings(string input, string expected)
        {
            var result = StringLiteralParser.Parser.Parse(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(@"""""", "")]
        [InlineData(@"""  """, "  ")]
        [InlineData(@"""hello""", "hello")]
        [InlineData(@"""world""", "world")]
        [InlineData(@"""  hello, world!  """, "  hello, world!  ")]
        public void can_parse_double_quoted_strings(string input, string expected)
        {
            var result = StringLiteralParser.Parser.Parse(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(@"'hello \'friend\''", "hello 'friend'")]
        [InlineData(@"""hello \""friend\""""", @"hello ""friend""")]
        [InlineData(@"'foo\\bar'", @"foo\bar")]
        [InlineData(@"""foo\\bar""", @"foo\bar")]
        public void can_parse_escaped_strings(string input, string expected)
        {
            var result = StringLiteralParser.Parser.Parse(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void cannot_parse_empty_input()
        {
            var result = StringLiteralParser.Parser(new Input(""));
            Assert.False(result.WasSuccessful);
            Assert.Equal("unexpected end of input", result.Message);
            Assert.Equal(2, result.Expectations.Count());
            Assert.Equal("string delimited by \"", result.Expectations.ElementAt(0));
            Assert.Equal("string delimited by '", result.Expectations.ElementAt(1));
        }

        [Theory]
        [InlineData("  ")]
        [InlineData("hello")]
        [InlineData("world")]
        public void cannot_parse_non_delimitered_string(string input)
        {
            var result = StringLiteralParser.Parser(new Input(input));
            Assert.False(result.WasSuccessful);
            Assert.Equal("unexpected '" + input[0] + "'", result.Message);
            Assert.Equal(2, result.Expectations.Count());
            Assert.Equal("\"", result.Expectations.ElementAt(0));
            Assert.Equal("'", result.Expectations.ElementAt(1));
        }

        [Theory]
        [InlineData('\'', "")]
        [InlineData('"', "")]
        [InlineData('\'', "hello")]
        [InlineData('"', "hello")]
        public void cannot_parse_non_terminated_string(char delimiter, string input)
        {
            var result = StringLiteralParser.Parser(new Input(delimiter + input));
            Assert.False(result.WasSuccessful);
            Assert.Equal("unexpected end of input", result.Message);
            Assert.Equal(1, result.Expectations.Count());
            Assert.Equal("continued string contents or " + delimiter, result.Expectations.ElementAt(0));
        }

        [Theory]
        [InlineData('\'', "\n")]
        [InlineData('\'', "\r")]
        [InlineData('\'', "\r\n")]
        [InlineData('"', "\n")]
        [InlineData('"', "\r")]
        [InlineData('"', "\r\n")]
        public void cannot_parse_newlines_within_literal(char delimiter, string newLine)
        {
            var result = StringLiteralParser.Parser(new Input(delimiter + "hello" + newLine + "world" + delimiter));
            Assert.False(result.WasSuccessful);
            Assert.Equal("unexpected end of line", result.Message);
            Assert.Equal(1, result.Expectations.Count());
            Assert.Equal("continued string contents or " + delimiter, result.Expectations.ElementAt(0));
        }
    }
}