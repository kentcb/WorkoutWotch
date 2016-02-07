namespace WorkoutWotch.Services.Contracts.Delay
{
    using System;
    using System.Reactive;

    public interface IDelayService
    {
        IObservable<Unit> Delay(TimeSpan duration);
    }
}