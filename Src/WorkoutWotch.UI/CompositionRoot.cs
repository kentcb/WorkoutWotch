namespace WorkoutWotch.UI
{
    using System;
    using System.Reactive.Concurrency;
    using System.Threading;
    using Akavache;
    using Genesis.Logging;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.ViewModels;

    public abstract class CompositionRoot
    {
        private readonly ILogger logger;

        // singletons
        protected readonly Lazy<IScheduler> backgroundScheduler;
        protected readonly Lazy<IBlobCache> blobCache;
        protected readonly Lazy<IAudioService> audioService;
        protected readonly Lazy<IDelayService> delayService;
        protected readonly Lazy<IExerciseDocumentService> exerciseDocumentService;
        protected readonly Lazy<IScheduler> mainScheduler;
        protected readonly Lazy<ISpeechService> speechService;
        protected readonly Lazy<IStateService> stateService;
        protected readonly Lazy<App> app;
        protected readonly Lazy<MainViewModel> mainViewModel;
        protected readonly Lazy<ExerciseProgramsViewModel> exerciseProgramsViewModel;

        protected CompositionRoot()
        {
            this.logger = LoggerService.GetLogger(this.GetType());

            this.backgroundScheduler = new Lazy<IScheduler>(this.CreateBackgroundScheduler);
            this.blobCache = new Lazy<IBlobCache>(this.CreateBlobCache);
            this.audioService = new Lazy<IAudioService>(this.CreateAudioService);
            this.delayService = new Lazy<IDelayService>(this.CreateDelayService);
            this.exerciseDocumentService = new Lazy<IExerciseDocumentService>(this.CreateExerciseDocumentService);
            this.mainScheduler = new Lazy<IScheduler>(this.CreateMainScheduler);
            this.speechService = new Lazy<ISpeechService>(this.CreateSpeechService);
            this.stateService = new Lazy<IStateService>(this.CreateStateService);
            this.app = new Lazy<App>(this.CreateApp);
            this.mainViewModel = new Lazy<MainViewModel>(this.CreateMainViewModel);
            this.exerciseProgramsViewModel = new Lazy<ExerciseProgramsViewModel>(this.CreateExerciseProgramsViewModel);
        }

        public IStateService ResolveStateService() =>
            this.stateService.Value;

        public App ResolveApp() =>
            this.app.Value;

        public MainViewModel ResolveMainViewModel() =>
            this.mainViewModel.Value;

        private IBlobCache CreateBlobCache() =>
            BlobCache.UserAccount;

        protected abstract IAudioService CreateAudioService();

        private IDelayService CreateDelayService() =>
            this.LoggedCreation(
                () =>
                    new DelayService(
                        this.mainScheduler.Value));

        private IScheduler CreateBackgroundScheduler() =>
            this.LoggedCreation(
                () =>
                    new EventLoopScheduler());

        protected abstract IExerciseDocumentService CreateExerciseDocumentService();

        private IScheduler CreateMainScheduler() =>
            new SynchronizationContextScheduler(SynchronizationContext.Current);

        protected abstract ISpeechService CreateSpeechService();

        private IStateService CreateStateService() =>
            this.LoggedCreation(
                () =>
                    new StateService(
                        this.blobCache.Value));

        private App CreateApp() =>
            this.LoggedCreation(
                () =>
                    new App(
                        this.mainViewModel.Value));

        private MainViewModel CreateMainViewModel() =>
            this.LoggedCreation(
                () =>
                    new MainViewModel(
                        () => this.exerciseProgramsViewModel.Value));

        private ExerciseProgramsViewModel CreateExerciseProgramsViewModel() =>
            this.LoggedCreation(
                () =>
                    new ExerciseProgramsViewModel(
                        this.audioService.Value,
                        this.delayService.Value,
                        this.exerciseDocumentService.Value,
                        this.mainScheduler.Value,
                        this.backgroundScheduler.Value,
                        this.speechService.Value,
                        this.stateService.Value,
                        this.mainViewModel.Value,
                        this.ExerciseProgramViewModelFactory));

        private ExerciseProgramViewModel ExerciseProgramViewModelFactory(ExerciseProgram model) =>
            this.LoggedCreation(
                () =>
                    new ExerciseProgramViewModel(
                        this.mainScheduler.Value,
                        this.mainViewModel.Value,
                        model));

        protected T LoggedCreation<T>(Func<T> factory)
        {
            this.logger.Debug("Instance of {0} requested.", typeof(T).FullName);

            using (this.logger.Perf("Create {0}.", typeof(T).FullName))
            {
                return factory();
            }
        }
    }
}