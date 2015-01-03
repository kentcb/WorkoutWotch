namespace WorkoutWotch.UI.iOS
{
    using Akavache;
    using TinyIoC;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.Services.iOS.Audio;
    using WorkoutWotch.Services.iOS.ExerciseDocument;
    using WorkoutWotch.Services.iOS.Speech;
    using WorkoutWotch.Services.iOS.SystemNotifications;
    using WorkoutWotch.Services.Logger;
    using WorkoutWotch.Services.Scheduler;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.ViewModels;

    public partial class AppDelegate
    {
        private void RegisterServices(TinyIoCContainer container)
        {
            container.Register<IBlobCache>(BlobCache.UserAccount);
            container.Register<IAudioService, AudioService>();
            container.Register<IContainerService>(container);
            container.Register<IDelayService, DelayService>();
            container.Register<IExerciseDocumentService, iCloudExerciseDocumentService>();
            container.Register<ILoggerService, LoggerService>();
            container.Register<ISchedulerService, SchedulerService>();
            container.Register<ISpeechService, SpeechService>();
            container.Register<IStateService, StateService>();
            container.Register<ISystemNotificationsService, SystemNotificationsService>();
        }

        private void RegisterViewModels(TinyIoCContainer container)
        {
            container.Register<ExerciseProgramsViewModel>().AsSingleton();
        }
    }
}