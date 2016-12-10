namespace WorkoutWotch.UnitTests.ViewModels.Builders
{
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Genesis.TestUtil;
    using global::ReactiveUI;
    using PCLMock;
    using ReactiveUI.Mocks;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Speech.Mocks;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using WorkoutWotch.ViewModels;

    public sealed class ExerciseProgramsViewModelBuilder : IBuilder
    {
        private bool activation;
        private IAudioService audioService;
        private IDelayService delayService;
        private IExerciseDocumentService exerciseDocumentService;
        private IScheduler mainScheduler;
        private IScheduler backgroundScheduler;
        private ISpeechService speechService;
        private IStateService stateService;
        private IScreen hostScreen;

        public ExerciseProgramsViewModelBuilder()
        {
            this.activation = true;
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
            this.exerciseDocumentService = new ExerciseDocumentServiceMock(MockBehavior.Loose);
            this.mainScheduler = CurrentThreadScheduler.Instance;
            this.backgroundScheduler = CurrentThreadScheduler.Instance;
            this.speechService = new SpeechServiceMock(MockBehavior.Loose);
            this.stateService = new StateServiceMock(MockBehavior.Loose);
            this.hostScreen = new ScreenMock(MockBehavior.Loose);

            this.WithCachedDocument(null);
        }

        public ExerciseProgramsViewModelBuilder WithActivation(bool activation) =>
            this.With(ref this.activation, activation);

        public ExerciseProgramsViewModelBuilder WithAudioService(IAudioService audioService) =>
            this.With(ref this.audioService, audioService);

        public ExerciseProgramsViewModelBuilder WithDelayService(IDelayService delayService) =>
            this.With(ref this.delayService, delayService);

        public ExerciseProgramsViewModelBuilder WithExerciseDocumentService(IExerciseDocumentService exerciseDocumentService) =>
            this.With(ref this.exerciseDocumentService, exerciseDocumentService);

        public ExerciseProgramsViewModelBuilder WithMainScheduler(IScheduler mainScheduler) =>
            this.With(ref this.mainScheduler, mainScheduler);

        public ExerciseProgramsViewModelBuilder WithBackgroundScheduler(IScheduler backgroundScheduler) =>
            this.With(ref this.backgroundScheduler, backgroundScheduler);

        public ExerciseProgramsViewModelBuilder WithScheduler(IScheduler scheduler) =>
            this
                .WithMainScheduler(scheduler)
                .WithBackgroundScheduler(scheduler);

        public ExerciseProgramsViewModelBuilder WithSpeechService(ISpeechService speechService) =>
            this.With(ref this.speechService, speechService);

        public ExerciseProgramsViewModelBuilder WithStateService(IStateService stateService) =>
            this.With(ref this.stateService, stateService);

        public ExerciseProgramsViewModelBuilder WithHostScreen(IScreen hostScreen) =>
            this.With(ref this.hostScreen, hostScreen);

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
                .When(x => x.Get<string>(It.IsAny<string>()))
                .Return(Observable.Return(cachedDocument));

            stateService
                .When(x => x.Set<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Observables.Unit);

            return this.WithStateService(stateService);
        }

        public ExerciseProgramsViewModel Build()
        {
            var result = new ExerciseProgramsViewModel(
                this.audioService,
                this.delayService,
                this.exerciseDocumentService,
                this.mainScheduler,
                this.backgroundScheduler,
                this.speechService,
                this.stateService,
                this.hostScreen,
                new ExerciseProgramViewModelFactoryBuilder().WithScheduler(this.mainScheduler));

            if (this.activation)
            {
                result.Activator.Activate();
            }

            return result;
        }

        public static implicit operator ExerciseProgramsViewModel(ExerciseProgramsViewModelBuilder builder) =>
            builder.Build();
    }
}