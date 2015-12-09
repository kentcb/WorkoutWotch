namespace WorkoutWotch.Services.Android.ExerciseDocument
{
    using System;
    using System.Reactive;
    using global::Android.Gms.Common;

    public interface IConnectionResultHandler
    {
        IObservable<bool> HandleConnectionResult(ConnectionResult connectionResult);
    }
}