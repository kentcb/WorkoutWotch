namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System.Reactive.Concurrency;
    using Genesis.TestUtil;
    using global::ReactiveUI;
    using Models.Builders;
    using PCLMock;
    using ReactiveUI.Mocks;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseProgramViewModelBuilder : IBuilder
    {
        private bool activation;
        private IScheduler scheduler;
        private IScreen hostScreen;
        private ExerciseProgram model;

        public ExerciseProgramViewModelBuilder()
        {
            this.activation = true;
            this.scheduler = new SchedulerMock(MockBehavior.Loose);
            this.hostScreen = new ScreenMock(MockBehavior.Loose);
            this.model = new ExerciseProgramBuilder();
        }

        public ExerciseProgramViewModelBuilder WithActivation(bool activation) =>
            this.With(ref this.activation, activation);

        public ExerciseProgramViewModelBuilder WithScheduler(IScheduler scheduler) =>
            this.With(ref this.scheduler, scheduler);

        public ExerciseProgramViewModelBuilder WithHostScreen(IScreen hostScreen) =>
            this.With(ref this.hostScreen, hostScreen);

        public ExerciseProgramViewModelBuilder WithModel(ExerciseProgram model) =>
            this.With(ref this.model, model);

        public ExerciseProgramViewModel Build()
        {
            var result = new ExerciseProgramViewModel(
                this.scheduler,
                this.hostScreen,
                this.model);

            if (this.activation)
            {
                result.Activator.Activate();
            }

            return result;
        }

        public static implicit operator ExerciseProgramViewModel(ExerciseProgramViewModelBuilder builder) =>
            builder.Build();
    }
}