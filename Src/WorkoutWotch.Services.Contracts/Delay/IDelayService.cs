using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkoutWotch.Services.Contracts.Delay
{
    public interface IDelayService
    {
        Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken));
    }
}

