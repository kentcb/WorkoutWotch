namespace WorkoutWotch.Services.Contracts.Delay
{
    using System;
    using System.Reactive;
    using System.Threading;

    public interface IDelayService
    {
        IObservable<Unit> DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken));
    }
}