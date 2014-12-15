namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Sprache;
    using WorkoutWotch.Models.Events;

    internal static class NumericalConstraintParser
    {
        // parses a literal number from the input, like "3" or "10"
        private static Parser<int> GetLiteralParser()
        {
            return Parse
                .Number
                .Select(int.Parse);
        }

        // parses the symbol "first" into a function that returns the number of the first item (be it set or rep or whatever) given the current event
        private static Parser<Func<T, int>> GetFirstSymbolParser<T>(Func<ExecutionContext, int> getFirst)
            where T : NumberedEvent
        {
            return Parse
                .IgnoreCase("first")
                .Select(x => (Func<T, int>)(e => getFirst(e.ExecutionContext)));
        }

        // parses the symbol "last" into a function that returns the number of the last item (be it set or rep or whatever) given the current event
        private static Parser<Func<T, int>> GetLastSymbolParser<T>(Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return Parse
                .IgnoreCase("last")
                .Select(x => (Func<T, int>)(e => getLast(e.ExecutionContext)));
        }

        // parse the symbol "first" with an optional addition expression after it into a function that returns true if the expression matches the current number in the event
        private static Parser<Func<T, int>> GetFirstSymbolExpressionParser<T>(Func<ExecutionContext, int> getFirst)
            where T : NumberedEvent
        {
            return
                from first in GetFirstSymbolParser<T>(getFirst)
                from numberToAdd in Parse.Char('+').Token(Parse.WhiteSpace.Except(NewLineParser.Parser)).Then(_ => Parse.Number.Select(int.Parse)).Optional()
                select (Func<T, int>)(e => first(e) + (numberToAdd.IsDefined ? numberToAdd.Get() : 0));
        }

        // parse the symbol "last" with an optional subtraction expression after it into a function that returns true if the expression matches the current number in the event
        private static Parser<Func<T, int>> GetLastSymbolExpressionParser<T>(Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return
                from last in GetLastSymbolParser<T>(getLast)
                from numberToSubtract in Parse.Char('-').Token(Parse.WhiteSpace.Except(NewLineParser.Parser)).Then(_ => Parse.Number.Select(int.Parse)).Optional()
                select (Func<T, int>)(e => last(e) - (numberToSubtract.IsDefined ? numberToSubtract.Get() : 0));
        }

        // parses either a literal number (e.g. "13") or a symbol with optional expression (e.g. "first", "first + 2") and returns the integral value
        private static Parser<Func<T, int>> GetNumberParser<T>(Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return GetLiteralParser()
                .Select(x => (Func<T, int>)(_ => x))
                .Or(GetFirstSymbolExpressionParser<T>(getFirst))
                .Or(GetLastSymbolExpressionParser<T>(getLast));
        }

        // parses either a literal number (e.g. "13") or a symbol with optional expression (e.g. "first", "first + 2") and returns a boolean
        // indicating whether the number matches the number in given event
        private static Parser<Func<T, bool>> GetNumberMatchParser<T>(
                Func<ExecutionContext, int> getCurrent,
                Func<ExecutionContext, int> getFirst,
                Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return
                from getNumber in GetNumberParser<T>(getFirst, getLast)
                select (Func<T, bool>)(e => getCurrent(e.ExecutionContext) == getNumber(e));
        }

        // parses a range (e.g. "1..5" or "1..2..10") into a function that returns true if the current number in the given event falls into that range
        private static Parser<Func<T, bool>> GetRangeParser<T>(Func<ExecutionContext, int> getCurrent, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return
                from getFirstNumber in GetNumberParser<T>(getFirst, getLast)
                from getSecondNumber in Parse.String("..").Token(Parse.WhiteSpace.Except(NewLineParser.Parser)).Then(_ => GetNumberParser<T>(getFirst, getLast))
                from getThirdNumber in Parse.String("..").Token(Parse.WhiteSpace.Except(NewLineParser.Parser)).Then(_ => GetNumberParser<T>(getFirst, getLast)).Optional()
                let getStart = getFirstNumber
                let getEnd = getThirdNumber.IsDefined ? getThirdNumber.Get() : getSecondNumber
                let getSkip = getThirdNumber.IsDefined ? getSecondNumber : (Func<T, int>)(_ => 1)
                select (Func<T, bool>)(e =>
                {
                    var startIndex = getStart(e);
                    var endIndex = getEnd(e);
                    var skipCount = getSkip(e);

                    for (var i = startIndex; i <= endIndex; i += skipCount)
                    {
                        if (getCurrent(e.ExecutionContext) == i)
                        {
                            return true;
                        }
                    }

                    return false;
                });
        }

        private static Parser<Func<T, bool>> GetAtomParser<T>(Func<ExecutionContext, int> getCurrent, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return GetRangeParser<T>(getCurrent, getFirst, getLast)
                .Or(GetNumberMatchParser<T>(getCurrent, getFirst, getLast));
        }

        private static Parser<Func<T, bool>> GetMathematicalSetParser<T>(Func<ExecutionContext, int> getCurrent, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return GetAtomParser<T>(getCurrent, getFirst, getLast)
                .DelimitedBy(Parse.Char(',').Token(Parse.WhiteSpace.Except(NewLineParser.Parser)))
                .Select(x => (Func<T, bool>)(e =>
                {
                    foreach (var atom in x)
                    {
                        if (atom(e))
                        {
                            return true;
                        }
                    }

                    return false;
                }));
        }

        private static Parser<Func<T, bool>> GetExpressionParser<T>(Func<ExecutionContext, int> getCurrent, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return
                from not in Parse.Char('^').Then(_ => Parse.WhiteSpace.Except(NewLineParser.Parser).Many()).Optional()
                from mathematicalSet in GetMathematicalSetParser<T>(getCurrent, getFirst, getLast)
                select (Func<T, bool>)(e =>
                {
                    var result = mathematicalSet(e);

                    if (not.IsDefined)
                    {
                        result = !result;
                    }

                    return result;
                });
        }

        public static Parser<Func<T, bool>> GetParser<T>(Func<ExecutionContext, int> getCurrent, Func<ExecutionContext, int> getFirst, Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return GetExpressionParser<T>(getCurrent, getFirst, getLast);
        }
    }
}