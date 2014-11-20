using System;

namespace WorkoutWotch.Services.Contracts.ExerciseDocument
{
    public interface IExerciseDocumentService
    {
        IObservable<string> ExerciseDocument
        {
            get;
        }
    }
}