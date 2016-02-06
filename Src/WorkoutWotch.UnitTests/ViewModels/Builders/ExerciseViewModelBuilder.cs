namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System;
    using System.Reactive.Linq;
    using Models.Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseViewModelBuilder : IBuilder
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

        public ExerciseViewModelBuilder WithSchedulerService(ISchedulerService schedulerService) =>
            this.With(ref this.schedulerService, schedulerService);

        public ExerciseViewModelBuilder WithModel(Exercise model) =>
            this.With(ref this.model, model);

        public ExerciseViewModelBuilder WithExecutionContext(IObservable<ExecutionContext> executionContext) =>
            this.With(ref this.executionContext, executionContext);

        public ExerciseViewModelBuilder WithExecutionContext(ExecutionContext executionContext) =>
            this.WithExecutionContext(Observable.Return(executionContext));

        public ExerciseViewModel Build() =>
            new ExerciseViewModel(
                this.schedulerService,
                this.model,
                this.executionContext);

        public static implicit operator ExerciseViewModel(ExerciseViewModelBuilder builder) =>
            builder.Build();
    }
}