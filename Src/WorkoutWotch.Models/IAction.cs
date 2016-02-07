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

        IObservable<Unit> Execute(ExecutionContext context);
    }
}