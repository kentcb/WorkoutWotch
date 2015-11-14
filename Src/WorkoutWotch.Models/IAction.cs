namespace WorkoutWotch.Models
{
    using System;
    using System.Reactive;

    public interface IAction
    {
        TimeSpan Duration
        {
            get;
        }

        IObservable<Unit> ExecuteAsync(ExecutionContext context);
    }
}