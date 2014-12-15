namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Parsers;

    [TestFixture]
    public class NumericalConstraintParserFixture
    {
        [TestCase("1", 1, 1, 4, true)]
        [TestCase("3", 3, 1, 4, true)]
        [TestCase("3", 2, 1, 4, false)]
        [TestCase("2", 3, 1, 4, false)]
        public void can_parse_literal_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.AreEqual(expected, result(new ExecutionContext()));
        }

        [TestCase("first", 1, 1, 4, true)]
        [TestCase("last", 4, 1, 4, true)]
        [TestCase("first", 1, 1, 1, true)]
        [TestCase("last", 1, 1, 1, true)]
        [TestCase("first", 2, 1, 4, false)]
        [TestCase("last", 3, 1, 4, false)]
        [TestCase("First", 1, 1, 4, true)]
        [TestCase("LAST", 4, 1, 4, true)]
        public void can_parse_symbol_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.AreEqual(expected, result(new ExecutionContext()));
        }

        [TestCase("first+0", 1, 1, 4, true)]
        [TestCase("last-0", 4, 1, 4, true)]
        [TestCase("first+1", 2, 1, 4, true)]
        [TestCase("last-1", 3, 1, 4, true)]
        [TestCase("first+2", 3, 1, 4, true)]
        [TestCase("last-2", 2, 1, 4, true)]
        [TestCase("first +  2", 3, 1, 4, true)]
        [TestCase("last  \t - 2", 2, 1, 4, true)]
        [TestCase("first+1", 1, 1, 4, false)]
        [TestCase("last-1", 4, 1, 4, false)]
        public void can_parse_symbol_expression_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.AreEqual(expected, result(new ExecutionContext()));
        }

        [TestCase("1..3", 1, 1, 4, true)]
        [TestCase("1..3", 2, 1, 4, true)]
        [TestCase("1..3", 3, 1, 4, true)]
        [TestCase("1..3", 4, 1, 4, false)]
        [TestCase("3..1", 2, 1, 4, false)]
        [TestCase("1..2..5", 1, 1, 5, true)]
        [TestCase("1..2..5", 2, 1, 5, false)]
        [TestCase("1..2..5", 3, 1, 5, true)]
        [TestCase("1..2..5", 4, 1, 5, false)]
        [TestCase("1..2..5", 5, 1, 5, true)]
        [TestCase("2..2..5", 2, 1, 5, true)]
        [TestCase("2..2..5", 3, 1, 5, false)]
        [TestCase("1..3..5", 1, 1, 5, true)]
        [TestCase("1..3..5", 2, 1, 5, false)]
        [TestCase("1..3..5", 3, 1, 5, false)]
        [TestCase("1..3..5", 4, 1, 5, true)]
        [TestCase("first..2..last", 4, 1, 5, false)]
        [TestCase("first..2..last", 5, 1, 5, true)]
        [TestCase("first..2..last-1", 5, 1, 5, false)]
        [TestCase("1  .. \t 3", 2, 1, 4, true)]
        [TestCase("2..3", 1, 1, 4, false)]
        public void can_parse_range_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.AreEqual(expected, result(new ExecutionContext()));
        }

        [TestCase("1,2,3", 1, 1, 4, true)]
        [TestCase("1,2,3", 2, 1, 4, true)]
        [TestCase("1,2,3", 3, 1, 4, true)]
        [TestCase("1,2,3", 4, 1, 4, false)]
        [TestCase("2,3", 1, 1, 4, false)]
        [TestCase("first,last,first+2..2..last-2", 1, 1, 10, true)]
        [TestCase("first,last,first+2..2..last-2", 2, 1, 10, false)]
        [TestCase("first,last,first+2..2..last-2", 3, 1, 10, true)]
        [TestCase("first,last,first+2..2..last-2", 4, 1, 10, false)]
        [TestCase("1 ,  2 \t , \t  3", 3, 1, 4, true)]
        public void can_parse_mathematical_set_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.AreEqual(expected, result(new ExecutionContext()));
        }

        [TestCase("^1", 1, 1, 4, false)]
        [TestCase("^1", 2, 1, 4, true)]
        [TestCase("^1", 3, 1, 4, true)]
        [TestCase("^first", 1, 1, 4, false)]
        [TestCase("^first", 2, 1, 4, true)]
        [TestCase("^last", 4, 1, 4, false)]
        [TestCase("^last", 3, 1, 4, true)]
        [TestCase("^first+1", 1, 1, 4, true)]
        [TestCase("^first+1", 2, 1, 4, false)]
        [TestCase("^2..2..10", 1, 1, 10, true)]
        [TestCase("^2..2..10", 2, 1, 10, false)]
        [TestCase("^2..2..10", 3, 1, 10, true)]
        [TestCase("^2..2..10", 4, 1, 10, false)]
        [TestCase("^first+1..2..last", 1, 1, 10, true)]
        [TestCase("^first+1..2..last", 2, 1, 10, false)]
        [TestCase("^  \t first  + 1  .. 2  \t ..  last", 1, 1, 10, true)]
        public void can_parse_not_constraints(string input, int actual, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser(ec => actual, ec => first, ec => last).Parse(input);
            Assert.AreEqual(expected, result(new ExecutionContext()));
        }

        [TestCase("")]
        [TestCase("frist")]
        [TestCase("1..\n3")]
        [TestCase("first,\nlast")]
        [TestCase("first\n,last")]
        [TestCase("^\n3")]
        [TestCase("first+\n1")]
        [TestCase("last-\n1")]
        [TestCase("first-1")]
        [TestCase("last+1")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = NumericalConstraintParser.GetParser(ec => 0, ec => 0, ec => 0)(new Input(input));
            Assert.True(!result.WasSuccessful || !result.Remainder.AtEnd);
        }
    }
}