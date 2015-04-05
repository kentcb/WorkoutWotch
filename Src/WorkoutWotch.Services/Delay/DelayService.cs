namespace WorkoutWotch.Services.Delay
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class DelayService : IDelayService
    {
        public async Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken))
            =>
                await Task
                    .Delay(duration, cancellationToken)
                    .ContinueOnAnyContext();
    }
}