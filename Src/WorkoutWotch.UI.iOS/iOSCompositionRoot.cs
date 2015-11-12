namespace WorkoutWotch.UI.iOS
{
    using System;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.iOS.Audio;
    using WorkoutWotch.Services.iOS.ExerciseDocument;
    using WorkoutWotch.Services.iOS.Speech;
    using WorkoutWotch.Services.iOS.SystemNotifications;
    using WorkoutWotch.UI.iOS.Views.ExercisePrograms;

    public sealed class iOSCompositionRoot : CompositionRoot
    {
        private readonly Lazy<ExerciseProgramsHostView> exerciseProgramsHostView;

        public iOSCompositionRoot()
        {
            this.exerciseProgramsHostView = new Lazy<ExerciseProgramsHostView>(this.CreateExerciseProgramsHostView);
        }

        public ExerciseProgramsHostView ResolveExerciseProgramsHostView() =>
            this.exerciseProgramsHostView.Value;

        private ExerciseProgramsHostView CreateExerciseProgramsHostView() =>
            new ExerciseProgramsHostView(
                this.exerciseProgramsViewModel.Value);

        protected override IAudioService CreateAudioService() =>
            new AudioService();

        protected override IExerciseDocumentService CreateExerciseDocumentService() =>
            //new iCloudExerciseDocumentService(
            //    this.loggerService.Value);
            // comment the above lines and uncomment this line if you want return a "canned" exercise document that can only be changed in code
            // this is useful if you don't want to have to set up iCloud integration
            new CannedExerciseDocumentService();

        protected override ISpeechService CreateSpeechService() =>
            new SpeechService();

        protected override ISystemNotificationsService CreateSystemNotificationsService() =>
            new SystemNotificationsService();
    }
}