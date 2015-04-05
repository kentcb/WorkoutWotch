namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Sprache;

    internal static class NumericalConstraintParser
    {
        // parses and returns a literal number from the input, like "3" or "10"
        private static Parser<int> GetLiteralParser()
            =>
                Parse
                    .Number
                    .Select(int.Parse);

        // parses the symbol "first" into a function that returns the number of the first item (be it set or rep or whatever) given an execution context
        private static Parser<Func<ExecutionContext, int>> GetFirstSymbolParser(Func<ExecutionContext, int> getFirst)
            => 
                Parse
                    .IgnoreCase("first")
                    .Select(x => getFirst);

        // parses the symbol "last" into a function that returns the number of the last item (be it set or rep or whatever) given an execution context
        private static Parser<Func<ExecutionContext, int>> GetLastSymbolParser(Func<ExecutionContext, int> getLast)
            => 
                Parse
                    .IgnoreCase("last")
                    .Select(x => getLast);

        // parses the symbol "first" with an optional addition expression after it. e.g. "first+1" or "first+3"
        // returns a function that gets the numerical value of that expression given an execution context
        private static Parser<Func<ExecutionContext, int>> GetFirstSymbolExpressionParser(Func<ExecutionContext, int> getFirst)
            => 
                from first in GetFirstSymbolParser(getFirst)
                from numberToAdd in Parse.Char('+').Token(HorizontalWhitespaceParser.Parser).Then(_ => Parse.Number.Select(int.Parse)).Optional()
                select (Func<ExecutionContext, int>)(ec => first(ec) + (numberToAdd.IsDefined ? numberToAdd.Get() : 0));

        // parses the symbol "last" with an optional subtraciton expression after it. e.g. "last-1" or "last-3"
        // returns a function that gets the numerical value of that expression given an execution context
        private static Parser<Func<ExecutionContext, int>> GetLastSymbolExpressionParser(Func<ExecutionContext, int> getLast)
            => 
                from last in GetLastSymbolParser(getLast)
                from numberToSubtract in Parse.Char('-').Token(HorizontalWhitespaceParser.Parser).Then(_ => Parse.Number.Select(int.Parse)).Optional()
                select (Func<ExecutionContext, int>)(ec => last(ec) - (numberToSubtract.IsDefined ? numberToSubtract.Get() : 0));

        // parses either a literal number or a symbol expression. e.g. "3", "first", "first + 2"
        // returns a function that gets the numerical value of that expression given an execution context
        private static Parser<Func<ExecutionContext, int>> GetNumberParser(Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            => 
                GetLiteralParser()
                    .Select(x => (Func<ExecutionContext, int>)(_ => x))
                    .Or(GetFirstSymbolExpressionParser(getFirst))
                    .Or(GetLastSymbolExpressionParser(getLast));

        // parses either a literal number or a symbol expression. e.g. "3", "first", "first + 2"
        // returns a function that returns true if the current number in a given execution context matches that expression
        private static Parser<Func<ExecutionContext, bool>> GetNumberMatchParser(
                Func<ExecutionContext, int> getActual,
                Func<ExecutionContext, int> getFirst,
                Func<ExecutionContext, int> getLast)
            => 
                from getNumber in GetNumberParser(getFirst, getLast)
                select (Func<ExecutionContext, bool>)(ec => getActual(ec) == getNumber(ec));

        // parses a range. e.g. "1..5", "1..2..10"
        // returns a function that returns true if the current number in the execution context falls within that range
        private static Parser<Func<ExecutionContext, bool>> GetRangeParser(Func<ExecutionContext, int> getActual, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            => 
                from getFirstNumber in GetNumberParser(getFirst, getLast)
                from getSecondNumber in Parse.String("..").Token(HorizontalWhitespaceParser.Parser).Then(_ => GetNumberParser(getFirst, getLast))
                from getThirdNumber in Parse.String("..").Token(HorizontalWhitespaceParser.Parser).Then(_ => GetNumberParser(getFirst, getLast)).Optional()
                let getStart = getFirstNumber
                let getEnd = getThirdNumber.IsDefined ? getThirdNumber.Get() : getSecondNumber
                let getSkip = getThirdNumber.IsDefined ? getSecondNumber : (Func<ExecutionContext, int>)(_ => 1)
                select (Func<ExecutionContext, bool>)(ec =>
                {
                    var startIndex = getStart(ec);
                    var endIndex = getEnd(ec);
                    var skipCount = getSkip(ec);

                    for (var i = startIndex; i <= endIndex; i += skipCount)
                    {
                        if (getActual(ec) == i)
                        {
                            return true;
                        }
                    }

                    return false;
                });

        // parses an atom. That is, the smallest recognized "unit". e.g. "1..3", "first", "2", "last-1", "3..2..8"
        // returns a function that returns true if the current number in the execution context matches the atom
        private static Parser<Func<ExecutionContext, bool>> GetAtomParser(Func<ExecutionContext, int> getActual, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            => 
                GetRangeParser(getActual, getFirst, getLast)
                    .Or(GetNumberMatchParser(getActual, getFirst, getLast));

        // parses a set of atoms. e.g. "1, 2, 3, 5, 8", "1..3, 5..9, last"
        // returns a function that returns true if the current number in the execution context is in the set
        private static Parser<Func<ExecutionContext, bool>> GetMathematicalSetParser(Func<ExecutionContext, int> getActual, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            => 
                GetAtomParser(getActual, getFirst, getLast)
                    .DelimitedBy(Parse.Char(',').Token(HorizontalWhitespaceParser.Parser))
                    .Select(x => (Func<ExecutionContext, bool>)(ec =>
                    {
                        foreach (var atom in x)
                        {
                            if (atom(ec))
                            {
                                return true;
                            }
                        }

                        return false;
                    }));

        // parses an expression recognized by the numerical constraint parser. e.g. "3", "first", "last-1", "4..8", "first..2..last", "1, 2, 5", "first, first+2..last-2, last"
        // returns a function that returns true if the current number in the execution context matches that expression
        private static Parser<Func<ExecutionContext, bool>> GetExpressionParser(Func<ExecutionContext, int> getActual, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            => 
                from not in Parse.Char('^').Then(_ => HorizontalWhitespaceParser.Parser.Many()).Optional()
                from mathematicalSet in GetMathematicalSetParser(getActual, getFirst, getLast)
                select (Func<ExecutionContext, bool>)(ec =>
                {
                    var result = mathematicalSet(ec);

                    if (not.IsDefined)
                    {
                        result = !result;
                    }

                    return result;
                });

        public static Parser<Func<ExecutionContext, bool>> GetParser(Func<ExecutionContext, int> getActual, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            => GetExpressionParser(getActual, getFirst, getLast);
    }
}