namespace WorkoutWotch.UI.iOS
{
    using Services.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.iOS.Audio;
    using Services.iOS.ExerciseDocument;
    using WorkoutWotch.Services.iOS.Speech;

    public sealed class iOSCompositionRoot : CompositionRoot
    {
        protected override IAudioService CreateAudioService() =>
            this.LoggedCreation(
                () =>
                    new AudioService(
                        this.mainScheduler.Value));

        protected override IExerciseDocumentService CreateExerciseDocumentService() =>
            this.LoggedCreation(
                () =>
                    // just used canned data - useful for getting you up and running quickly
                    new CannedExerciseDocumentService());
                    // comment the above lines and uncomment this line if you want to use an iCloud-based document service
                    //new iCloudExerciseDocumentService(this.loggerService.Value));

        protected override ISpeechService CreateSpeechService() =>
            this.LoggedCreation(
                () =>
                    new SpeechService());
    }
}