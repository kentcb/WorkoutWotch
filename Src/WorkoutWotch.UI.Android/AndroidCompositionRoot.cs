namespace WorkoutWotch.UI.Android
{
    using Services.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Android.Audio;
    using WorkoutWotch.Services.Android.Speech;
    using Services.Android.ExerciseDocument;

    public sealed class AndroidCompositionRoot : CompositionRoot
    {
        private readonly MainActivity mainActivity;

        public AndroidCompositionRoot(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        protected override IAudioService CreateAudioService() =>
            new AudioService();

        protected override IExerciseDocumentService CreateExerciseDocumentService() =>
            // just used canned data - useful for getting you up and running quickly
            new CannedExerciseDocumentService();
            // comment the above lines and uncomment this line if you want to use an iCloud-based document service
            //new GoogleDriveExerciseDocumentService(
            //    this.loggerService.Value,
            //    this.mainActivity);

        protected override ISpeechService CreateSpeechService() =>
            new SpeechService();
    }
}