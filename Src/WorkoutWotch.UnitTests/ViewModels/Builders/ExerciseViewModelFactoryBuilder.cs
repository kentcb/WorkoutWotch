namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System.Reactive.Concurrency;
    using Genesis.TestUtil;
    using PCLMock;
    using Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseViewModelFactoryBuilder : IBuilder
    {
        private IScheduler scheduler;

        public ExerciseViewModelFactoryBuilder()
        {
            this.scheduler = new SchedulerMock(MockBehavior.Loose);
        }

        public ExerciseViewModelFactoryBuilder WithScheduler(IScheduler scheduler) =>
            this.With(ref this.scheduler, scheduler);

        public ExerciseViewModelFactory Build() =>
            (model, executionContext) =>
                new ExerciseViewModelBuilder()
                    .WithScheduler(this.scheduler)
                    .WithExecutionContext(executionContext)
                    .WithModel(model);

        public static implicit operator ExerciseViewModelFactory(ExerciseViewModelFactoryBuilder builder) =>
            builder.Build();
    }
}