namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using global::ReactiveUI;
    using Kent.Boogaart.PCLMock;
    using Models.Builders;
    using ReactiveUI.Mocks;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseProgramViewModelBuilder
    {
        private ILoggerService loggerService;
        private ISchedulerService schedulerService;
        private IScreen hostScreen;
        private ExerciseProgram model;

        public ExerciseProgramViewModelBuilder()
        {
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);
            this.hostScreen = new ScreenMock(MockBehavior.Loose);
            this.model = new ExerciseProgramBuilder();
        }

        public ExerciseProgramViewModelBuilder WithLoggerService(ILoggerService loggerService)
        {
            this.loggerService = loggerService;
            return this;
        }

        public ExerciseProgramViewModelBuilder WithSchedulerService(ISchedulerService schedulerService)
        {
            this.schedulerService = schedulerService;
            return this;
        }

        public ExerciseProgramViewModelBuilder WithHostScreen(IScreen hostScreen)
        {
            this.hostScreen = hostScreen;
            return this;
        }

        public ExerciseProgramViewModelBuilder WithModel(ExerciseProgram model)
        {
            this.model = model;
            return this;
        }

        public ExerciseProgramViewModel Build() =>
            new ExerciseProgramViewModel(
                this.loggerService,
                this.schedulerService,
                this.hostScreen,
                this.model);
        
        public static implicit operator ExerciseProgramViewModel(ExerciseProgramViewModelBuilder builder) =>
            builder.Build();
    }
}