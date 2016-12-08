namespace WorkoutWotch.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Utility;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class Exercise
    {
        private readonly ILogger logger;
        private readonly ISpeechService speechService;
        private readonly string name;
        private readonly int setCount;
        private readonly int repetitionCount;
        private readonly IImmutableList<MatcherWithAction> matchersWithActions;
        private readonly TimeSpan duration;

        public Exercise(ILoggerService loggerService, ISpeechService speechService, string name, int setCount, int repetitionCount, IEnumerable<MatcherWithAction> matchersWithActions)
        {
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));
            Ensure.ArgumentNotNull(name, nameof(name));
            Ensure.ArgumentNotNull(matchersWithActions, nameof(matchersWithActions));
            Ensure.ArgumentCondition(setCount >= 0, "setCount cannot be less than zero.", "setCount");
            Ensure.ArgumentCondition(repetitionCount >= 0, "repetitionCount cannot be less than zero.", "repetitionCount");

            this.logger = loggerService.GetLogger(this.GetType());
            this.speechService = speechService;
            this.name = name;
            this.setCount = setCount;
            this.repetitionCount = repetitionCount;
            this.matchersWithActions = matchersWithActions.ToImmutableList();

            var dummyExecutionContext = new ExecutionContext();
            this.duration = this
                .GetEventsWithActions(dummyExecutionContext)
                .SelectMany(x => x.Actions)
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
        }

        public string Name => this.name;

        public int SetCount => this.setCount;

        public int RepetitionCount => this.repetitionCount;

        public IImmutableList<MatcherWithAction> MatchersWithActions => this.matchersWithActions;

        public TimeSpan Duration => this.duration;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            return Observable
                .Concat(
                    this
                        .GetEventsWithActions(context)
                        .SelectMany(eventWithActions => eventWithActions.Actions.Select(action => new { Action = action, Event = eventWithActions.Event }))
                        .Select(
                            actionAndEvent =>
                            {
                                var action = actionAndEvent.Action;
                                var @event = actionAndEvent.Event;

                                if (context.SkipAhead > TimeSpan.Zero && context.SkipAhead >= action.Duration)
                                {
                                    this.logger.Debug("Skipping action {0} for event {1} because its duration ({2}) is less than the remaining skip ahead ({3}).", action, @event, action.Duration, context.SkipAhead);
                                    context.AddProgress(action.Duration);
                                    return Observable.Return(Unit.Default);
                                }

                                this.logger.Debug("Executing action {0} for event {1}.", action, @event);
                                return action.Execute(context);
                            }));
        }

        private IEnumerable<IEvent> GetEvents(ExecutionContext executionContext)
        {
            executionContext.SetCurrentExercise(this);

            yield return new BeforeExerciseEvent(executionContext, this);

            for (var setNumber = 1; setNumber <= this.SetCount; ++setNumber)
            {
                executionContext.SetCurrentSet(setNumber);

                yield return new BeforeSetEvent(executionContext, setNumber);

                for (var repetitionNumber = 1; repetitionNumber <= this.RepetitionCount; ++repetitionNumber)
                {
                    executionContext.SetCurrentRepetition(repetitionNumber);

                    yield return new BeforeRepetitionEvent(executionContext, repetitionNumber);
                    yield return new DuringRepetitionEvent(executionContext, repetitionNumber);
                    yield return new AfterRepetitionEvent(executionContext, repetitionNumber);
                }

                yield return new AfterSetEvent(executionContext, setNumber);
            }

            yield return new AfterExerciseEvent(executionContext, this);
        }

        private IEnumerable<EventWithActions> GetEventsWithActions(ExecutionContext executionContext) =>
            this
                .GetEvents(executionContext)
                .Select(x => new EventWithActions(x, this.GetActionsForEvent(x)));

        private IEnumerable<IAction> GetActionsForEvent(IEvent @event)
        {
            BeforeExerciseEvent beforeExerciseEvent;

            if ((beforeExerciseEvent = @event as BeforeExerciseEvent) != null)
            {
                yield return new SayAction(this.speechService, beforeExerciseEvent.Exercise.Name);
            }

            foreach (var matcherWithAction in this.matchersWithActions)
            {
                if (!matcherWithAction.Matcher.Matches(@event))
                {
                    continue;
                }

                yield return matcherWithAction.Action;
            }
        }

        private struct EventWithActions
        {
            private readonly IEvent @event;
            private readonly IImmutableList<IAction> actions;

            public EventWithActions(IEvent @event, IEnumerable<IAction> actions)
            {
                this.@event = @event;
                this.actions = actions.ToImmutableList();
            }

            public IEvent Event => this.@event;

            public IImmutableList<IAction> Actions => this.actions;
        }
    }
}