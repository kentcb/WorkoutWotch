namespace WorkoutWotch.Services.Delay
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Contracts.Scheduler;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class DelayService : IDelayService
    {
        private readonly ISchedulerService schedulerService;

        public DelayService(ISchedulerService schedulerService)
        {
            schedulerService.AssertNotNull(nameof(schedulerService));
            this.schedulerService = schedulerService;
        }

        public IObservable<Unit> DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken)) =>
            Observable
                .Return(Unit.Default)
                .Delay(duration, schedulerService.TaskPoolScheduler)
                .RunAsync(cancellationToken);
    }
}