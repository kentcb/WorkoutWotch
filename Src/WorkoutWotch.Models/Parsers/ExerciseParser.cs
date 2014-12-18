namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class ExerciseParser
    {
        private static readonly Parser<string> nameParser =
            from _ in Parse.String("##")
            from __ in Parse.WhiteSpace.Except(NewLineParser.Parser).AtLeastOnce()
            from name in Parse.AnyChar.Except(NewLineParser.Parser).AtLeastOnce()
            from ___ in NewLineParser.Parser
            let nameString = new string(name.ToArray()).TrimEnd()
            select nameString;

        private static readonly Parser<Tuple<int, int>> setAndRepetitionCountParser =
            from _ in Parse.String("*")
            from setCount in Parse.Number.Select(int.Parse).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from __ in Parse.IgnoreCase("set").Then(x => Parse.IgnoreCase("s").Optional()).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from ___ in Parse.IgnoreCase("x").Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from repetitionCount in Parse.Number.Select(int.Parse).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from ____ in Parse.IgnoreCase("rep").Then(x => Parse.IgnoreCase("s").Optional()).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from _____ in NewLineParser.Parser
            select Tuple.Create(setCount, repetitionCount);

        private static Parser<MatcherWithAction> GetTypedEventMatcherWithActionParser<T>(string name, bool nameCanBePluralized, IContainerService containerService)
            where T : IEvent
        {
            return
                from _ in Parse.String("*")
                from __ in Parse.WhiteSpace.Except(NewLineParser.Parser)
                from ___ in Parse.IgnoreCase(name)
                from ____ in nameCanBePluralized ? Parse.IgnoreCase("s").Optional() : ParseExt.Default<IOption<IEnumerable<char>>>()
                from _____ in Parse.Char(':')
                from ______ in NewLineParser.Parser
                from actions in ActionListParser.GetParser(1, containerService)
                let action = new SequenceAction(actions)
                select new MatcherWithAction(new TypedEventMatcher<T>(), action);
        }

        private static Parser<MatcherWithAction> GetNumberedEventMatcherWithActionParser<T>(
                string name,
                bool nameCanBePluralized,
                IContainerService containerService,
                Func<ExecutionContext, int> getActual,
                Func<ExecutionContext, int> getFirst,
                Func<ExecutionContext, int> getLast)
            where T : NumberedEvent
        {
            return
                from _ in Parse.String("*")
                from __ in Parse.WhiteSpace.Except(NewLineParser.Parser)
                from ___ in Parse.IgnoreCase(name)
                from ____ in nameCanBePluralized ? Parse.IgnoreCase("s").Optional() : ParseExt.Default<IOption<IEnumerable<char>>>()
                from _____ in Parse.WhiteSpace.Except(NewLineParser.Parser).AtLeastOnce()
                from numericalConstraint in NumericalConstraintParser.GetParser(getActual, getFirst, getLast)
                from ______ in Parse.String(":").Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
                from _______ in NewLineParser.Parser
                from actions in ActionListParser.GetParser(1, containerService)
                let action = new SequenceAction(actions)
                select new MatcherWithAction(new NumberedEventMatcher<T>(e => numericalConstraint(e.ExecutionContext)), action);
        }

        private static Parser<MatcherWithAction> GetMatcherWithActionParser(IContainerService containerService)
        {
            return GetTypedEventMatcherWithActionParser<BeforeExerciseEvent>("Before", false, containerService)
                .Or(GetTypedEventMatcherWithActionParser<AfterExerciseEvent>("After", false, containerService))
                .Or(GetTypedEventMatcherWithActionParser<BeforeSetEvent>("Before set", true, containerService))
                .Or(GetTypedEventMatcherWithActionParser<AfterSetEvent>("After set", true, containerService))
                .Or(GetTypedEventMatcherWithActionParser<BeforeRepetitionEvent>("Before rep", true, containerService))
                .Or(GetTypedEventMatcherWithActionParser<DuringRepetitionEvent>("During rep", true, containerService))
                .Or(GetTypedEventMatcherWithActionParser<AfterRepetitionEvent>("After rep", true, containerService))
                .Or(GetNumberedEventMatcherWithActionParser<BeforeSetEvent>("Before set", true, containerService, ec => ec.CurrentSet, ec => 1, ec => ec.CurrentExercise.SetCount))
                .Or(GetNumberedEventMatcherWithActionParser<AfterSetEvent>("After set", true, containerService, ec => ec.CurrentSet, ec => 1, ec => ec.CurrentExercise.SetCount))
                .Or(GetNumberedEventMatcherWithActionParser<BeforeRepetitionEvent>("Before rep", true, containerService, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount))
                .Or(GetNumberedEventMatcherWithActionParser<DuringRepetitionEvent>("During rep", true, containerService, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount))
                .Or(GetNumberedEventMatcherWithActionParser<AfterRepetitionEvent>("After rep", true, containerService, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount));
        }

        private static Parser<IEnumerable<MatcherWithAction>> GetMatchersWithActionsParser(IContainerService containerService)
        {
            return GetMatcherWithActionParser(containerService).DelimitedBy(NewLineParser.Parser);
        }

        public static Parser<Exercise> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from name in nameParser
                from setAndRepetitionCount in setAndRepetitionCountParser
                from matchersWithActions in GetMatchersWithActionsParser(containerService).Optional()
                select new Exercise(
                    containerService.Resolve<ILoggerService>(),
                    containerService.Resolve<ISpeechService>(),
                    name,
                    setAndRepetitionCount.Item1,
                    setAndRepetitionCount.Item2,
                    matchersWithActions.GetOrElse(Enumerable.Empty<MatcherWithAction>()));
        }
    }
}