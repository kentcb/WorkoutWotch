namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using Kent.Boogaart.PCLMock;
    using Models.Builders;
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
        private ExerciseProgram model;

        public ExerciseProgramViewModelBuilder()
        {
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);
            this.model = new ExerciseProgramBuilder().Build();
        }

        public ExerciseProgramViewModel Build()
        {
            return new ExerciseProgramViewModel(
                this.loggerService,
                this.schedulerService,
                this.model);
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

        public ExerciseProgramViewModelBuilder WithModel(ExerciseProgram model)
        {
            this.model = model;
            return this;
        }
    }
}