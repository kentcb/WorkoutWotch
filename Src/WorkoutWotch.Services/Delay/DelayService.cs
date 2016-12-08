namespace WorkoutWotch.Services.Delay
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class DelayService : IDelayService
    {
        private readonly IScheduler scheduler;

        public DelayService(IScheduler scheduler)
        {
            Ensure.ArgumentNotNull(scheduler, nameof(scheduler));
            this.scheduler = scheduler;
        }

        public IObservable<Unit> Delay(TimeSpan duration) =>
            Observables
                .Unit
                .Delay(duration, scheduler);
    }
}