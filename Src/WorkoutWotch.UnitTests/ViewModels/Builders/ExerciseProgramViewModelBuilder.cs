namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System.Reactive.Concurrency;
    using global::ReactiveUI;
    using Models.Builders;
    using PCLMock;
    using ReactiveUI.Mocks;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseProgramViewModelBuilder : IBuilder
    {
        private ILoggerService loggerService;
        private IScheduler scheduler;
        private IScreen hostScreen;
        private ExerciseProgram model;

        public ExerciseProgramViewModelBuilder()
        {
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.scheduler = new SchedulerMock(MockBehavior.Loose);
            this.hostScreen = new ScreenMock(MockBehavior.Loose);
            this.model = new ExerciseProgramBuilder();
        }

        public ExerciseProgramViewModelBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public ExerciseProgramViewModelBuilder WithScheduler(IScheduler scheduler) =>
            this.With(ref this.scheduler, scheduler);

        public ExerciseProgramViewModelBuilder WithHostScreen(IScreen hostScreen) =>
            this.With(ref this.hostScreen, hostScreen);

        public ExerciseProgramViewModelBuilder WithModel(ExerciseProgram model) =>
            this.With(ref this.model, model);

        public ExerciseProgramViewModel Build() =>
            new ExerciseProgramViewModel(
                this.loggerService,
                this.scheduler,
                this.hostScreen,
                this.model);
        
        public static implicit operator ExerciseProgramViewModel(ExerciseProgramViewModelBuilder builder) =>
            builder.Build();
    }
}