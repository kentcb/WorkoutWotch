namespace WorkoutWotch.Services.Contracts.Delay
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDelayService
    {
        Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken));
    }
}