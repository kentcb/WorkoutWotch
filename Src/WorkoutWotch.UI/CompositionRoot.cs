namespace WorkoutWotch.UI
{
    using System;
    using Akavache;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.Services.Logger;
    using WorkoutWotch.Services.Scheduler;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.ViewModels;

    public abstract class CompositionRoot
    {
        // singletons
        protected readonly Lazy<IBlobCache> blobCache;
        protected readonly Lazy<IAudioService> audioService;
        protected readonly Lazy<IDelayService> delayService;
        protected readonly Lazy<IExerciseDocumentService> exerciseDocumentService;
        protected readonly Lazy<ILoggerService> loggerService;
        protected readonly Lazy<ISchedulerService> schedulerService;
        protected readonly Lazy<ISpeechService> speechService;
        protected readonly Lazy<IStateService> stateService;
        protected readonly Lazy<App> app;
        protected readonly Lazy<MainViewModel> mainViewModel;
        protected readonly Lazy<ExerciseProgramsViewModel> exerciseProgramsViewModel;

        protected CompositionRoot()
        {
            this.blobCache = new Lazy<IBlobCache>(this.CreateBlobCache);
            this.audioService = new Lazy<IAudioService>(this.CreateAudioService);
            this.delayService = new Lazy<IDelayService>(this.CreateDelayService);
            this.exerciseDocumentService = new Lazy<IExerciseDocumentService>(this.CreateExerciseDocumentService);
            this.loggerService = new Lazy<ILoggerService>(this.CreateLoggerService);
            this.schedulerService = new Lazy<ISchedulerService>(this.CreateSchedulerService);
            this.speechService = new Lazy<ISpeechService>(this.CreateSpeechService);
            this.stateService = new Lazy<IStateService>(this.CreateStateService);
            this.app = new Lazy<App>(this.CreateApp);
            this.mainViewModel = new Lazy<MainViewModel>(this.CreateMainViewModel);
            this.exerciseProgramsViewModel = new Lazy<ExerciseProgramsViewModel>(this.CreateExerciseProgramsViewModel);
        }

        public ILoggerService ResolveLoggerService() =>
            this.loggerService.Value;

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
            new DelayService(
                this.schedulerService.Value);

        protected abstract IExerciseDocumentService CreateExerciseDocumentService();

        private ILoggerService CreateLoggerService() =>
            new LoggerService();

        private ISchedulerService CreateSchedulerService() =>
            new SchedulerService();

        protected abstract ISpeechService CreateSpeechService();

        private IStateService CreateStateService() =>
            new StateService(
                this.blobCache.Value,
                this.loggerService.Value);

        private App CreateApp() =>
            new App(
                this.mainViewModel.Value);

        private MainViewModel CreateMainViewModel() =>
            new MainViewModel(
                () => this.exerciseProgramsViewModel.Value);

        private ExerciseProgramsViewModel CreateExerciseProgramsViewModel() =>
            new ExerciseProgramsViewModel(
                this.audioService.Value,
                this.delayService.Value,
                this.exerciseDocumentService.Value,
                this.loggerService.Value,
                this.schedulerService.Value,
                this.speechService.Value,
                this.stateService.Value,
                this.mainViewModel.Value,
                this.ExerciseProgramViewModelFactory);

        private ExerciseProgramViewModel ExerciseProgramViewModelFactory(ExerciseProgram model) =>
            new ExerciseProgramViewModel(
                this.loggerService.Value,
                this.schedulerService.Value,
                this.mainViewModel.Value,
                model);
    }
}