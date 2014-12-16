namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;

    internal static class ExerciseParser
    {
        private static readonly Parser<string> nameParser =
            from _ in Parse.String("##")
            from name in Parse.AnyChar.Until(NewLineParser.Parser)
            select new string(name.ToArray()).Trim();

        private static readonly Parser<Tuple<int, int>> setAndRepetitionCountParser =
            from _ in Parse.String("*")
            from setCount in Parse.Number.Select(int.Parse).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from __ in Parse.IgnoreCase("set").Then(x => Parse.IgnoreCase("s").Optional()).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from ___ in Parse.IgnoreCase("x").Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from repetitionCount in Parse.Number.Select(int.Parse).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from ____ in Parse.IgnoreCase("rep").Then(x => Parse.IgnoreCase("s").Optional()).Token(Parse.WhiteSpace.Except(NewLineParser.Parser))
            from _____ in NewLineParser.Parser
            select Tuple.Create(setCount, repetitionCount);

        private static Parser<MatcherWithAction> GetTypedEventMatcherWithActionParser<T>(
                string name,
                bool nameCanBePluralized,
                IAudioService audioService,
                IDelayService delayService,
                ILoggerService loggerService,
                ISpeechService speechService)
            where T : IEvent
        {
            return
                from _ in Parse.String("*")
                from __ in Parse.WhiteSpace.Except(NewLineParser.Parser)
                from ___ in Parse.IgnoreCase(name)
                from ____ in nameCanBePluralized ? Parse.IgnoreCase("s").Optional() : ParseExt.Default<IOption<IEnumerable<char>>>()
                from _____ in Parse.Char(':')
                from ______ in NewLineParser.Parser
                from actions in ActionListParser.GetParser(1, audioService, delayService, loggerService, speechService)
                let action = new SequenceAction(actions)
                select new MatcherWithAction(new TypedEventMatcher<T>(), action);
        }

        private static Parser<MatcherWithAction> GetNumberedEventMatcherWithActionParser<T>(
                string name,
                bool nameCanBePluralized,
                IAudioService audioService,
                IDelayService delayService,
                ILoggerService loggerService,
                ISpeechService speechService,
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
                from actions in ActionListParser.GetParser(1, audioService, delayService, loggerService, speechService)
                let action = new SequenceAction(actions)
                select new MatcherWithAction(new NumberedEventMatcher<T>(e => numericalConstraint(e.ExecutionContext)), action);
        }

        private static Parser<MatcherWithAction> GetMatcherWithActionParser(
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            return GetTypedEventMatcherWithActionParser<BeforeExerciseEvent>("Before", false, audioService, delayService, loggerService, speechService)
                .Or(GetTypedEventMatcherWithActionParser<AfterExerciseEvent>("After", false, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<BeforeSetEvent>("Before set", true, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<AfterSetEvent>("After set", true, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<BeforeRepetitionEvent>("Before rep", true, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<DuringRepetitionEvent>("During rep", true, audioService, delayService, loggerService, speechService))
                .Or(GetTypedEventMatcherWithActionParser<AfterRepetitionEvent>("After rep", true, audioService, delayService, loggerService, speechService))
                .Or(GetNumberedEventMatcherWithActionParser<BeforeSetEvent>("Before set", true, audioService, delayService, loggerService, speechService, ec => ec.CurrentSet, ec => 1, ec => ec.CurrentExercise.SetCount))
                .Or(GetNumberedEventMatcherWithActionParser<AfterSetEvent>("After set", true, audioService, delayService, loggerService, speechService, ec => ec.CurrentSet, ec => 1, ec => ec.CurrentExercise.SetCount))
                .Or(GetNumberedEventMatcherWithActionParser<BeforeRepetitionEvent>("Before rep", true, audioService, delayService, loggerService, speechService, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount))
                .Or(GetNumberedEventMatcherWithActionParser<DuringRepetitionEvent>("During rep", true, audioService, delayService, loggerService, speechService, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount))
                .Or(GetNumberedEventMatcherWithActionParser<AfterRepetitionEvent>("After rep", true, audioService, delayService, loggerService, speechService, ec => ec.CurrentRepetition, ec => 1, ec => ec.CurrentExercise.RepetitionCount));
        }

        private static Parser<IEnumerable<MatcherWithAction>> GetMatchersWithActionsParser(
            IAudioService audioService,
            IDelayService delayService,
            ILoggerService loggerService,
            ISpeechService speechService)
        {
            return GetMatcherWithActionParser(audioService, delayService, loggerService, speechService).DelimitedBy(NewLineParser.Parser);
        }

        public static Parser<Exercise> GetParser(IAudioService audioService, IDelayService delayService, ILoggerService loggerService, ISpeechService speechService)
        {
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");
            speechService.AssertNotNull("speechService");

            return
                from name in nameParser
                from setAndRepetitionCount in setAndRepetitionCountParser
                from matchersWithActions in GetMatchersWithActionsParser(audioService, delayService, loggerService, speechService).Optional()
                select new Exercise(
                    loggerService,
                    speechService,
                    name,
                    setAndRepetitionCount.Item1,
                    setAndRepetitionCount.Item2,
                    matchersWithActions.GetOrElse(Enumerable.Empty<MatcherWithAction>()));
        }
    }
}