namespace WorkoutWotch.Services.ExerciseDocument
{
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using System.Reflection;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;

    public sealed class CannedExerciseDocumentService : IExerciseDocumentService
    {
        public IObservable<string> ExerciseDocument => Observable.Return(GetDefaultExerciseDocument());

        private static string GetDefaultExerciseDocument()
        {
            using (var stream = typeof(CannedExerciseDocumentService).GetTypeInfo().Assembly.GetManifestResourceStream("WorkoutWotch.Services.ExerciseDocument.DefaultExerciseDocument.mkd"))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}