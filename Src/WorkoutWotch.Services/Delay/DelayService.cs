using System;
using WorkoutWotch.Services.Contracts.Delay;
using System.Threading.Tasks;

namespace WorkoutWotch.Services.Delay
{
    public sealed class DelayService : IDelayService
    {
        public async System.Threading.Tasks.Task DelayAsync(TimeSpan duration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            await Task.Delay(duration, cancellationToken)
                .ContinueOnAnyContext();
        }
    }
}

