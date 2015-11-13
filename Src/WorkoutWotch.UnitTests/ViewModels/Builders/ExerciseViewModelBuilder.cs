namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System;
    using System.Reactive.Linq;
    using Kent.Boogaart.PCLMock;
    using Models.Builders;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseViewModelBuilder
    {
        private ISchedulerService schedulerService;
        private Exercise model;
        private IObservable<ExecutionContext> executionContext;

        public ExerciseViewModelBuilder()
        {
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);
            this.model = new ExerciseBuilder();
            this.executionContext = Observable.Never<ExecutionContext>();
        }

        public ExerciseViewModelBuilder WithSchedulerService(ISchedulerService schedulerService)
        {
            this.schedulerService = schedulerService;
            return this;
        }

        public ExerciseViewModelBuilder WithModel(Exercise model)
        {
            this.model = model;
            return this;
        }

        public ExerciseViewModelBuilder WithExecutionContext(IObservable<ExecutionContext> executionContext)
        {
            this.executionContext = executionContext;
            return this;
        }

        public ExerciseViewModelBuilder WithExecutionContext(ExecutionContext executionContext)
        {
            this.executionContext = Observable.Return(executionContext);
            return this;
        }

        public ExerciseViewModel Build() =>
            new ExerciseViewModel(
                this.schedulerService,
                this.model,
                this.executionContext);

        public static implicit operator ExerciseViewModel(ExerciseViewModelBuilder builder) =>
            builder.Build();
    }
}