namespace WorkoutWotch.Services.Delay
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Contracts.Scheduler;
    using Utility;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class DelayService : IDelayService
    {
        private readonly ISchedulerService schedulerService;

        public DelayService(ISchedulerService schedulerService)
        {
            Ensure.ArgumentNotNull(schedulerService, nameof(schedulerService));
            this.schedulerService = schedulerService;
        }

        public IObservable<Unit> DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken)) =>
            Observable
                .Return(Unit.Default)
                .Delay(duration, schedulerService.TaskPoolScheduler)
                .RunAsync(cancellationToken);
    }
}