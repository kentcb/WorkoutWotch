namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Sprache;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public sealed class NumericalConstraintParserFixture
    {
        [Theory]
        [InlineData("1", 1, 1, 4, true)]
        [InlineData("3", 3, 1, 4, true)]
        [InlineData("3", 2, 1, 4, false)]
        [InlineData("2", 3, 1, 4, false)]
        public void can_parse_literal_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.Equal(expected, result(new ExecutionContext()));
        }

        [Theory]
        [InlineData("first", 1, 1, 4, true)]
        [InlineData("last", 4, 1, 4, true)]
        [InlineData("first", 1, 1, 1, true)]
        [InlineData("last", 1, 1, 1, true)]
        [InlineData("first", 2, 1, 4, false)]
        [InlineData("last", 3, 1, 4, false)]
        [InlineData("First", 1, 1, 4, true)]
        [InlineData("LAST", 4, 1, 4, true)]
        public void can_parse_symbol_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.Equal(expected, result(new ExecutionContext()));
        }

        [Theory]
        [InlineData("first+0", 1, 1, 4, true)]
        [InlineData("last-0", 4, 1, 4, true)]
        [InlineData("first+1", 2, 1, 4, true)]
        [InlineData("last-1", 3, 1, 4, true)]
        [InlineData("first+2", 3, 1, 4, true)]
        [InlineData("last-2", 2, 1, 4, true)]
        [InlineData("first +  2", 3, 1, 4, true)]
        [InlineData("last  \t - 2", 2, 1, 4, true)]
        [InlineData("first+1", 1, 1, 4, false)]
        [InlineData("last-1", 4, 1, 4, false)]
        public void can_parse_symbol_expression_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.Equal(expected, result(new ExecutionContext()));
        }

        [Theory]
        [InlineData("1..3", 1, 1, 4, true)]
        [InlineData("1..3", 2, 1, 4, true)]
        [InlineData("1..3", 3, 1, 4, true)]
        [InlineData("1..3", 4, 1, 4, false)]
        [InlineData("3..1", 2, 1, 4, false)]
        [InlineData("1..2..5", 1, 1, 5, true)]
        [InlineData("1..2..5", 2, 1, 5, false)]
        [InlineData("1..2..5", 3, 1, 5, true)]
        [InlineData("1..2..5", 4, 1, 5, false)]
        [InlineData("1..2..5", 5, 1, 5, true)]
        [InlineData("2..2..5", 2, 1, 5, true)]
        [InlineData("2..2..5", 3, 1, 5, false)]
        [InlineData("1..3..5", 1, 1, 5, true)]
        [InlineData("1..3..5", 2, 1, 5, false)]
        [InlineData("1..3..5", 3, 1, 5, false)]
        [InlineData("1..3..5", 4, 1, 5, true)]
        [InlineData("first..2..last", 4, 1, 5, false)]
        [InlineData("first..2..last", 5, 1, 5, true)]
        [InlineData("first..2..last-1", 5, 1, 5, false)]
        [InlineData("1  .. \t 3", 2, 1, 4, true)]
        [InlineData("2..3", 1, 1, 4, false)]
        public void can_parse_range_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.Equal(expected, result(new ExecutionContext()));
        }

        [Theory]
        [InlineData("1,2,3", 1, 1, 4, true)]
        [InlineData("1,2,3", 2, 1, 4, true)]
        [InlineData("1,2,3", 3, 1, 4, true)]
        [InlineData("1,2,3", 4, 1, 4, false)]
        [InlineData("2,3", 1, 1, 4, false)]
        [InlineData("first,last,first+2..2..last-2", 1, 1, 10, true)]
        [InlineData("first,last,first+2..2..last-2", 2, 1, 10, false)]
        [InlineData("first,last,first+2..2..last-2", 3, 1, 10, true)]
        [InlineData("first,last,first+2..2..last-2", 4, 1, 10, false)]
        [InlineData("1 ,  2 \t , \t  3", 3, 1, 4, true)]
        public void can_parse_mathematical_set_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.Equal(expected, result(new ExecutionContext()));
        }

        [Theory]
        [InlineData("^1", 1, 1, 4, false)]
        [InlineData("^1", 2, 1, 4, true)]
        [InlineData("^1", 3, 1, 4, true)]
        [InlineData("^first", 1, 1, 4, false)]
        [InlineData("^first", 2, 1, 4, true)]
        [InlineData("^last", 4, 1, 4, false)]
        [InlineData("^last", 3, 1, 4, true)]
        [InlineData("^first+1", 1, 1, 4, true)]
        [InlineData("^first+1", 2, 1, 4, false)]
        [InlineData("^2..2..10", 1, 1, 10, true)]
        [InlineData("^2..2..10", 2, 1, 10, false)]
        [InlineData("^2..2..10", 3, 1, 10, true)]
        [InlineData("^2..2..10", 4, 1, 10, false)]
        [InlineData("^first+1..2..last", 1, 1, 10, true)]
        [InlineData("^first+1..2..last", 2, 1, 10, false)]
        [InlineData("^  \t first  + 1  .. 2  \t ..  last", 1, 1, 10, true)]
        public void can_parse_not_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.Equal(expected, result(new ExecutionContext()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("frist")]
        [InlineData("first,\nlast")]
        [InlineData("first\n,last")]
        [InlineData("first+\n1")]
        [InlineData("last-\n1")]
        [InlineData("first-1")]
        [InlineData("last+1")]
        [InlineData("1..\n3")]
        [InlineData("1\n..3")]
        [InlineData("1. .3")]
        [InlineData("1..2..\n3")]
        [InlineData("1..2. .3")]
        [InlineData("1,\n2")]
        [InlineData("^\n3")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = NumericalConstraintParser.GetParser(ec => 0, ec => 0, ec => 0)(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}