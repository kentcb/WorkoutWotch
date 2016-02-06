namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using global::ReactiveUI;
    using Models.Builders;
    using PCLMock;
    using ReactiveUI.Mocks;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseProgramViewModelBuilder : IBuilder
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

        public ExerciseProgramViewModelBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public ExerciseProgramViewModelBuilder WithSchedulerService(ISchedulerService schedulerService) =>
            this.With(ref this.schedulerService, schedulerService);

        public ExerciseProgramViewModelBuilder WithHostScreen(IScreen hostScreen) =>
            this.With(ref this.hostScreen, hostScreen);

        public ExerciseProgramViewModelBuilder WithModel(ExerciseProgram model) =>
            this.With(ref this.model, model);

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