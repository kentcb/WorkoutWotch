namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System.Reactive.Concurrency;
    using Genesis.TestUtil;
    using PCLMock;
    using Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseProgramViewModelFactoryBuilder : IBuilder
    {
        private IScheduler scheduler;

        public ExerciseProgramViewModelFactoryBuilder()
        {
            this.scheduler = new SchedulerMock(MockBehavior.Loose);
        }

        public ExerciseProgramViewModelFactoryBuilder WithScheduler(IScheduler scheduler) =>
            this.With(ref this.scheduler, scheduler);

        public ExerciseProgramViewModelFactory Build() =>
            model =>
                new ExerciseProgramViewModelBuilder()
                    .WithScheduler(this.scheduler)
                    .WithModel(model);

        public static implicit operator ExerciseProgramViewModelFactory(ExerciseProgramViewModelFactoryBuilder builder) =>
            builder.Build();
    }
}