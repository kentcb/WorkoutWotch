using System.Reactive.Linq;

namespace WorkoutWotch.UnitTests.ViewModels
{
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseProgramsViewModelBuilder
    {
        private IContainerService containerService;
        private IExerciseDocumentService exerciseDocumentService;
        private ILoggerService loggerService;
        private ISchedulerService schedulerService;
        private IStateService stateService;

        public ExerciseProgramsViewModelBuilder()
        {
            this.containerService = new ContainerServiceMock(MockBehavior.Loose);
            this.exerciseDocumentService = new ExerciseDocumentServiceMock(MockBehavior.Loose);
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);

            this.WithCachedDocument(null);
        }

        public ExerciseProgramsViewModel Build()
        {
            return new ExerciseProgramsViewModel(
                this.containerService,
                this.exerciseDocumentService,
                this.loggerService,
                this.schedulerService,
                this.stateService);
        }

        public ExerciseProgramsViewModelBuilder WithContainerService(IContainerService containerService)
        {
            this.containerService = containerService;
            return this;
        }

        public ExerciseProgramsViewModelBuilder WithExerciseDocumentService(IExerciseDocumentService exerciseDocumentService)
        {
            this.exerciseDocumentService = exerciseDocumentService;
            return this;
        }

        public ExerciseProgramsViewModelBuilder WithLoggerService(ILoggerService loggerService)
        {
            this.loggerService = loggerService;
            return this;
        }

        public ExerciseProgramsViewModelBuilder WithSchedulerService(ISchedulerService schedulerService)
        {
            this.schedulerService = schedulerService;
            return this;
        }

        public ExerciseProgramsViewModelBuilder WithStateService(IStateService stateService)
        {
            this.stateService = stateService;
            return this;
        }

        public ExerciseProgramsViewModelBuilder WithCloudDocument(string cloudDocument)
        {
            var exerciseDocumentService = new ExerciseDocumentServiceMock();

            exerciseDocumentService
                .When(x => x.ExerciseDocument)
                .Return(Observable.Return(cloudDocument));

            return this.WithExerciseDocumentService(exerciseDocumentService);
        }

        public ExerciseProgramsViewModelBuilder WithCachedDocument(string cachedDocument)
        {
            var stateService = new StateServiceMock(MockBehavior.Loose);

            stateService
                .When(x => x.GetAsync<string>(It.IsAny<string>()))
                .Return(Task.FromResult(cachedDocument));

            stateService
                .When(x => x.SetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Task.FromResult(true));

            return this.WithStateService(stateService);
        }
    }
}