namespace WorkoutWotch.Services.iOS.ExerciseDocument
{
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;

    public sealed class CannedExerciseDocumentService : IExerciseDocumentService
    {
        public IObservable<string> ExerciseDocument
        {
            get { return Observable.Return(GetDefaultExerciseDocument()); }
        }

        private static string GetDefaultExerciseDocument()
        {
            using (var stream = typeof(iCloudExerciseDocumentService).Assembly.GetManifestResourceStream("WorkoutWotch.Services.iOS.ExerciseDocument.DefaultExerciseDocument.mkd"))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}