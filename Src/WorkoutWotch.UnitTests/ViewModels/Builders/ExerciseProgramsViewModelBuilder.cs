namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System.Reactive;
    using System.Reactive.Linq;
    using global::ReactiveUI;
    using PCLMock;
    using ReactiveUI.Mocks;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Speech.Mocks;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using WorkoutWotch.ViewModels;

    internal sealed class ExerciseProgramsViewModelBuilder : IBuilder
    {
        private IAudioService audioService;
        private IDelayService delayService;
        private IExerciseDocumentService exerciseDocumentService;
        private ILoggerService loggerService;
        private ISchedulerService schedulerService;
        private ISpeechService speechService;
        private IStateService stateService;
        private IScreen hostScreen;
        private ExerciseProgramViewModelFactory exerciseProgramViewModelFactory;

        public ExerciseProgramsViewModelBuilder()
        {
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
            this.exerciseDocumentService = new ExerciseDocumentServiceMock(MockBehavior.Loose);
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
            this.schedulerService = new SchedulerServiceMock(MockBehavior.Loose);
            this.speechService = new SpeechServiceMock(MockBehavior.Loose);
            this.stateService = new StateServiceMock(MockBehavior.Loose);
            this.hostScreen = new ScreenMock(MockBehavior.Loose);
            this.exerciseProgramViewModelFactory =
                model =>
                    new ExerciseProgramViewModelBuilder()
                        .WithModel(model)
                        .Build();

            this.WithCachedDocument(null);
        }

        public ExerciseProgramsViewModelBuilder WithAudioService(IAudioService audioService) =>
            this.With(ref this.audioService, audioService);

        public ExerciseProgramsViewModelBuilder WithDelayService(IDelayService delayService) =>
            this.With(ref this.delayService, delayService);

        public ExerciseProgramsViewModelBuilder WithExerciseDocumentService(IExerciseDocumentService exerciseDocumentService) =>
            this.With(ref this.exerciseDocumentService, exerciseDocumentService);

        public ExerciseProgramsViewModelBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public ExerciseProgramsViewModelBuilder WithSchedulerService(ISchedulerService schedulerService) =>
            this.With(ref this.schedulerService, schedulerService);

        public ExerciseProgramsViewModelBuilder WithSpeechService(ISpeechService speechService) =>
            this.With(ref this.speechService, speechService);

        public ExerciseProgramsViewModelBuilder WithStateService(IStateService stateService) =>
            this.With(ref this.stateService, stateService);

        public ExerciseProgramsViewModelBuilder WithHostScreen(IScreen hostScreen) =>
            this.With(ref this.hostScreen, hostScreen);

        public ExerciseProgramsViewModelBuilder WithExerciseProgramViewModelFactory(ExerciseProgramViewModelFactory exerciseProgramViewModelFactory) =>
            this.With(ref this.exerciseProgramViewModelFactory, exerciseProgramViewModelFactory);

        public ExerciseProgramsViewModelBuilder WithCloudDocument(string cloudDocument)
        {
            var exerciseDocumentService = new ExerciseDocumentServiceMock(MockBehavior.Loose);

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
                .Return(Observable.Return(cachedDocument));

            stateService
                .When(x => x.SetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));

            return this.WithStateService(stateService);
        }

        public ExerciseProgramsViewModel Build() =>
            new ExerciseProgramsViewModel(
                this.audioService,
                this.delayService,
                this.exerciseDocumentService,
                this.loggerService,
                this.schedulerService,
                this.speechService,
                this.stateService,
                this.hostScreen,
                this.exerciseProgramViewModelFactory);
        
        public static implicit operator ExerciseProgramsViewModel(ExerciseProgramsViewModelBuilder builder) =>
            builder.Build();
    }
}