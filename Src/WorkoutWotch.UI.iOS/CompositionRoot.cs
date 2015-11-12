namespace WorkoutWotch.UI.iOS
{
    using System;
    using Akavache;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.Services.iOS.SystemNotifications;
    using WorkoutWotch.Services.Logger;
    using WorkoutWotch.Services.Scheduler;
    using WorkoutWotch.Services.State;
    using ViewModels;

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
        protected readonly Lazy<ISystemNotificationsService> systemNotificationsService;
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
            this.systemNotificationsService = new Lazy<ISystemNotificationsService>(this.CreateSystemNotificationsService);
            this.exerciseProgramsViewModel = new Lazy<ExerciseProgramsViewModel>(this.CreateExerciseProgramsViewModel);
        }

        public ILoggerService ResolveLoggerService() =>
            this.loggerService.Value;

        public IStateService ResolveStateService() =>
            this.stateService.Value;

        public ISystemNotificationsService ResolveSystemNotificationsService() =>
            this.systemNotificationsService.Value;

        private IBlobCache CreateBlobCache() =>
            BlobCache.UserAccount;

        protected abstract IAudioService CreateAudioService();

        private IDelayService CreateDelayService() =>
            new DelayService();

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

        protected abstract ISystemNotificationsService CreateSystemNotificationsService();

        private ExerciseProgramsViewModel CreateExerciseProgramsViewModel() =>
            new ExerciseProgramsViewModel(
                this.audioService.Value,
                this.delayService.Value,
                this.exerciseDocumentService.Value,
                this.loggerService.Value,
                this.schedulerService.Value,
                this.speechService.Value,
                this.stateService.Value);
    }
}