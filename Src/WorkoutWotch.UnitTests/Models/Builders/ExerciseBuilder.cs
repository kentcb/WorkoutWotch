namespace WorkoutWotch.UnitTests.Models.Builders
{
    using System.Collections.Generic;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    internal sealed class ExerciseBuilder
    {
        private readonly IList<MatcherWithAction> matchersWithActions;
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

        public Exercise Build()
        {
            return new Exercise(
                this.loggerService,
                this.speechService,
                this.name,
                this.setCount,
                this.repetitionCount,
                this.matchersWithActions);
        }

        public ExerciseBuilder WithLoggerService(ILoggerService loggerService)
        {
            this.loggerService = loggerService;
            return this;
        }

        public ExerciseBuilder WithSpeechService(ISpeechService speechService)
        {
            this.speechService = speechService;
            return this;
        }

        public ExerciseBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public ExerciseBuilder WithSetCount(int setCount)
        {
            this.setCount = setCount;
            return this;
        }

        public ExerciseBuilder WithRepetitionCount(int repetitionCount)
        {
            this.repetitionCount = repetitionCount;
            return this;
        }

        public ExerciseBuilder AddMatcherWithAction(MatcherWithAction matcherWithAction)
        {
            this.matchersWithActions.Add(matcherWithAction);
            return this;
        }

        public ExerciseBuilder WithBeforeExerciseAction(IAction action)
        {
            var matcher = new EventMatcherMock();
            matcher.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            return this.AddMatcherWithAction(new MatcherWithAction(matcher, action));
        }

        public static implicit operator Exercise(ExerciseBuilder builder)
        {
            return builder.Build();
        }
    }
}