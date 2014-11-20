namespace WorkoutWotch.Services.Contracts.ExerciseDocument
{
    using System;

    public interface IExerciseDocumentService
    {
        IObservable<string> ExerciseDocument
        {
            get;
        }
    }
}