namespace WorkoutWotch.UnitTests.Models.Builders
{
    using System.Collections.Generic;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    internal sealed class ExerciseBuilder : IBuilder
    {
        private List<MatcherWithAction> matchersWithActions;
        private ILoggerService loggerService;
        private ISpeechService speechService;
        private string name;
        private int setCount;
        private int repetitionCount;

        public ExerciseBuilder()
        {
            this.matchersWithActions = new List<MatcherWithAction>();
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.speechService = new SpeechServiceMock(MockBehavior.Loose);
            this.name = "Name";
            this.setCount = 1;
            this.repetitionCount = 1;
        }

        public ExerciseBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public ExerciseBuilder WithSpeechService(ISpeechService speechService) =>
            this.With(ref this.speechService, speechService);

        public ExerciseBuilder WithName(string name) =>
            this.With(ref this.name, name);

        public ExerciseBuilder WithSetCount(int setCount) =>
            this.With(ref this.setCount, setCount);

        public ExerciseBuilder WithRepetitionCount(int repetitionCount) =>
            this.With(ref this.repetitionCount, repetitionCount);

        public ExerciseBuilder WithMatcherWithAction(MatcherWithAction matcherWithAction) =>
            this.With(ref this.matchersWithActions, matcherWithAction);

        public ExerciseBuilder WithMatchersWithActions(IEnumerable<MatcherWithAction> matchersWithActions) =>
            this.With(ref this.matchersWithActions, matchersWithActions);

        public ExerciseBuilder WithBeforeExerciseAction(IAction action)
        {
            var matcher = new EventMatcherMock();
            matcher.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            return this.WithMatcherWithAction(new MatcherWithAction(matcher, action));
        }

        public Exercise Build() =>
            new Exercise(
                this.loggerService,
                this.speechService,
                this.name,
                this.setCount,
                this.repetitionCount,
                this.matchersWithActions);

        public static implicit operator Exercise(ExerciseBuilder builder) =>
            builder.Build();
    }
}