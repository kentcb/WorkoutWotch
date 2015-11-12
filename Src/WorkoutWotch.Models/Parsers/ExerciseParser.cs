namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class ExerciseParser
    {
        // parses the set and rep count. e.g. "3 sets x 1 rep" or "10 sets x 5 reps"
        // returns a tuple where the first item is the set count and the second is the rep count
        private static readonly Parser<Tuple<int, int>> setAndRepetitionCountParser =
            from _ in Parse.String("*")
            from setCount in Parse.Number.Select(int.Parse).Token(HorizontalWhitespaceParser.Parser)
            from __ in Parse.IgnoreCase("set").Then(x => Parse.IgnoreCase("s").Optional()).Token(HorizontalWhitespaceParser.Parser)
            from ___ in Parse.IgnoreCase("x").Token(HorizontalWhitespaceParser.Parser)
            from repetitionCount in Parse.Number.Select(int.Parse).Token(HorizontalWhitespaceParser.Parser)
            from ____ in Parse.IgnoreCase("rep").Then(x => Parse.IgnoreCase("s").Optional()).Token(HorizontalWhitespaceParser.Parser)
            from _____ in NewLineParser.Parser.Or(ParseExt.Default<NewLineType>().End())
            select Tuple.Create(setCount, repetitionCount);

        // parses an event matcher name. e.g. "before" or "after set"
        private static Parser<Unit> GetEventMatcherName(EventMatcherPreposition preposition, EventMatcherNoun? noun = null)
        {
            var parser = Parse.IgnoreCase(preposition.ToString()).ToUnit();

            if (noun.HasValue)
            {
                parser = parser
                    .Then(_ => HorizontalWhitespaceParser.Parser.AtLeastOnce())
                    .Then(_ => Parse.IgnoreCase(noun.ToString()))
                    .Then(__ => Parse.IgnoreCase("s").Optional())
                    .ToUnit();
            }

            return parser;
        }

        // parses a typed event matcher. e.g. "before set: ..." or "after rep: ..."
        // returns a matcher with action, where all specified actions are enclosed in a single sequence action
        private static Parser<MatcherWithAction> GetTypedEventMatcherWithActionParser<T>(
                Parser<Unit> nameParser,
                IAudioService audioService,
                IDelayService delayService,
                ILoggerService loggerService,
                ISpeechService speechService)
            where T : IEvent => 
                from _ in Parse.String("*")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in nameParser
                from ____ in Parse.Char(':').Token(HorizontalWhitespaceParser.Parser)
                from _____ in NewLineParser.Parser
                from actions in ActionListParser.GetParser(1, audioService, delayService, loggerService, speechService)
                let action = new SequenceAction(actions)
                select new MatcherWithAction(new TypedEventMatcher<T>(), action);

        // parses a typed event matcher. e.g. "before set 3: ..." or "after reps first+1..last: ..."
        // returns a matcher with action, where all specified actions are enclosed in a single sequence action
        private static Parser<MatcherWithAction> GetNumberedEventMatcherWithActionParser<T>(
                Parser<Unit> nameParser,
                Func<ExecutionContext, int> getActual,
                Func<ExecutionContext, int> getFirst,
                Func<ExecutionContext, int> getLast,
                IAudioService audioService,
                IDelayService delayService,
                ILoggerService loggerService,
                ISpeechService speechService)
            where T : NumberedEvent => 
                from _ in Parse.String("*")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in nameParser
                from ____ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from numericalConstraint in NumericalConstraintParser.GetParser(getActual, getFirst, getLast)
                from _____ in Parse.String(":").Token(HorizontalWhitespaceParser.Parser)
                from ______ in NewLineParser.Parser
                from actions in ActionListParser.GetParser(1, audioService, delayService, loggerService, speechService)
                let action = new SequenceAction(actions)
                select new MatcherWithAction(new NumberedEventMatcher<T>(e => numericalConstraint(e.ExecutionContext)), action);

        // parses any matcher with an action. e.g. "before: ..." or "after sets 3..2..8: ..."
        // returns a matcher with action, where all specified actions are enclosed in a single sequence action
        private static Parser<MatcherWithAction> GetMatcherWithActionParser(
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            var beforeSetNameParser = GetEventMatcherName(EventMatcherPreposition.Before, EventMatcherNoun.Set);
            var afterSetNameParser = GetEventMatcherName(EventMatcherPreposition.After, EventMatcherNoun.Set);
            var beforeRepNameParser =  GetEventMatcherName(EventMatcherPreposition.Before, EventMatcherNoun.Rep);
            var duringRepNameParser =  GetEventMatcherName(EventMatcherPreposition.During, EventMatcherNoun.Rep);
            var afterRepNameParser =  GetEventMatcherName(EventMatcherPreposition.After, EventMatcherNoun.Rep);

            return GetTypedEventMatcherWithActionParser<BeforeExerciseEvent>(GetEventMatcherName(EventMatcherPreposition.Before), audioService, delayService, loggerService, speechService)
                .Or(GetTypedEventMatcherWithActionParser<AfterExerciseEvent>(GetEventMatcherName(EventMatcherPreposition.After), audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<BeforeSetEvent>(beforeSetNameParser, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<AfterSetEvent>(afterSetNameParser, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<BeforeRepetitionEvent>(beforeRepNameParser, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<DuringRepetitionEvent>(duringRepNameParser, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<AfterRepetitionEvent>(afterRepNameParser, audioService, delayService, loggerService, speechService))
                .Or(GetNumberedEventMatcherWithActionParser<BeforeSetEvent>(beforeSetNameParser,  ec => ec.CurrentSet, ec => 1, ec => ec.CurrentExercise.SetCount, audioService, delayService, loggerService, speechService))
                .Or(GetNumberedEventMatcherWithActionParser<AfterSetEvent>(afterSetNameParser, ec => ec.CurrentSet, ec => 1, ec => ec.CurrentExercise.SetCount, audioService, delayService, loggerService, speechService))
                .Or(GetNumberedEventMatcherWithActionParser<BeforeRepetitionEvent>(beforeRepNameParser, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount, audioService, delayService, loggerService, speechService))
                .Or(GetNumberedEventMatcherWithActionParser<DuringRepetitionEvent>(duringRepNameParser, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount, audioService, delayService, loggerService, speechService))
                .Or(GetNumberedEventMatcherWithActionParser<AfterRepetitionEvent>(afterRepNameParser, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount, audioService, delayService, loggerService, speechService));
        }

        // parses any number of matchers with their associated action
        private static Parser<IEnumerable<MatcherWithAction>> GetMatchersWithActionsParser(
                IAudioService audioService,
                IDelayService delayService,
                ILoggerService loggerService,
                ISpeechService speechService) =>
            GetMatcherWithActionParser(audioService, delayService, loggerService, speechService)
                .DelimitedBy(NewLineParser.Parser.Token(HorizontalWhitespaceParser.Parser)
                .AtLeastOnce());

        public static Parser<Exercise> GetParser(
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            audioService.AssertNotNull(nameof(audioService));
            delayService.AssertNotNull(nameof(delayService));
            loggerService.AssertNotNull(nameof(loggerService));
            speechService.AssertNotNull(nameof(speechService));

            return
                from name in HeadingParser.GetParser(2)
                from _ in VerticalSeparationParser.Parser
                from setAndRepetitionCount in setAndRepetitionCountParser
                from __ in VerticalSeparationParser.Parser
                from matchersWithActions in GetMatchersWithActionsParser(audioService, delayService, loggerService, speechService).Optional()
                select new Exercise(
                    loggerService,
                    speechService,
                    name,
                    setAndRepetitionCount.Item1,
                    setAndRepetitionCount.Item2,
                    matchersWithActions.GetOrElse(Enumerable.Empty<MatcherWithAction>()));
        }

        private enum EventMatcherPreposition
        {
            Before,
            During,
            After
        }

        private enum EventMatcherNoun
        {
            Set,
            Rep
        }
    }
}