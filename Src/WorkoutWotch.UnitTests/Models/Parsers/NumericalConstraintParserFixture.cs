using System;
using NUnit.Framework;
using WorkoutWotch.Models.Parsers;
using Sprache;
using WorkoutWotch.Models.Events;
using WorkoutWotch.Models;

namespace WorkoutWotch.UnitTests.Models.Parsers
{
    [TestFixture]
    public class NumericalConstraintParserFixture
    {
        [TestCase("1", 1, 1, 4, true)]
        [TestCase("3", 3, 1, 4, true)]
        [TestCase("3", 2, 1, 4, false)]
        [TestCase("2", 3, 1, 4, false)]
        public void can_parse_literal_constraints(string input, int current, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser<BeforeRepetitionEvent>(e => current, e => first, e => last).Parse(input);
            Assert.AreEqual(expected, result(new BeforeRepetitionEvent(new ExecutionContext(), current)));
        }

        [TestCase("first", 1, 1, 4, true)]
        [TestCase("last", 4, 1, 4, true)]
        [TestCase("first", 1, 1, 1, true)]
        [TestCase("last", 1, 1, 1, true)]
        [TestCase("first", 2, 1, 4, false)]
        [TestCase("last", 3, 1, 4, false)]
        [TestCase("First", 1, 1, 4, true)]
        [TestCase("LAST", 4, 1, 4, true)]
        public void can_parse_symbol_constraints(string input, int current, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser<BeforeRepetitionEvent>(e => current, e => first, e => last).Parse(input);
            Assert.AreEqual(expected, result(new BeforeRepetitionEvent(new ExecutionContext(), current)));
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
        public void can_parse_symbol_expression_constraints(string input, int current, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser<BeforeRepetitionEvent>(e => current, e => first, e => last).Parse(input);
            Assert.AreEqual(expected, result(new BeforeRepetitionEvent(new ExecutionContext(), current)));
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
        [TestCase("first..2..last", 4, 1, 5, false)]
        [TestCase("first..2..last", 5, 1, 5, true)]
        [TestCase("first..2..last-1", 5, 1, 5, false)]
        [TestCase("1  .. \t 3", 2, 1, 4, true)]
        [TestCase("2..3", 1, 1, 4, false)]
        public void can_parse_range_constraints(string input, int current, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser<BeforeRepetitionEvent>(e => current, e => first, e => last).Parse(input);
            Assert.AreEqual(expected, result(new BeforeRepetitionEvent(new ExecutionContext(), current)));
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
        public void can_parse_mathematical_set_constraints(string input, int current, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser<BeforeRepetitionEvent>(e => current, e => first, e => last).Parse(input);
            Assert.AreEqual(expected, result(new BeforeRepetitionEvent(new ExecutionContext(), current)));
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
        public void can_parse_not_constraints(string input, int current, int first, int last, bool expected)
        {
            var result = NumericalConstraintParser.GetParser<BeforeRepetitionEvent>(e => current, e => first, e => last).Parse(input);
            Assert.AreEqual(expected, result(new BeforeRepetitionEvent(new ExecutionContext(), current)));
        }
    }
}

