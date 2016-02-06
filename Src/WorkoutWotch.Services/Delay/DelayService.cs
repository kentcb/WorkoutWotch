namespace WorkoutWotch.Services.Delay
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using Utility;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class DelayService : IDelayService
    {
        private readonly IScheduler scheduler;

        public DelayService(IScheduler scheduler)
        {
            Ensure.ArgumentNotNull(scheduler, nameof(scheduler));
            this.scheduler = scheduler;
        }

        public IObservable<Unit> DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken)) =>
            Observable
                .Return(Unit.Default)
                .Delay(duration, scheduler)
                .RunAsync(cancellationToken);
    }
}