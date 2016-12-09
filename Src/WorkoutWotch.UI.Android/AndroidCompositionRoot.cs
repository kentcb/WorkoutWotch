namespace WorkoutWotch.UI.Android
{
    using WorkoutWotch.Services.Android.Audio;
    using WorkoutWotch.Services.Android.Speech;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class AndroidCompositionRoot : CompositionRoot
    {
        protected override IAudioService CreateAudioService() =>
            new AudioService();

        protected override IExerciseDocumentService CreateExerciseDocumentService() =>
            // just used canned data - useful for getting you up and running quickly
            new Services.ExerciseDocument.CannedExerciseDocumentService();
            // comment the above lines and uncomment this line if you want to use an iCloud-based document service
            //new Services.Android.ExerciseDocument.GoogleDriveExerciseDocumentService(
            //        (MainActivity)Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity);

        protected override ISpeechService CreateSpeechService() =>
            new SpeechService();
    }
}