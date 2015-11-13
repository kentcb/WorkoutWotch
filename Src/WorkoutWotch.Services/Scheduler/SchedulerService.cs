namespace WorkoutWotch.Services.Scheduler
{
    using System.Reactive.Concurrency;
    using System.Threading;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using Rx = System.Reactive.Concurrency;

    public sealed class SchedulerService : ISchedulerService
    {
        private readonly IScheduler mainScheduler;

        public SchedulerService()
        {
            this.mainScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
        }

        public IScheduler DefaultScheduler => Rx.DefaultScheduler.Instance;

        public IScheduler CurrentThreadScheduler => Rx.CurrentThreadScheduler.Instance;

        public IScheduler ImmediateScheduler => Rx.ImmediateScheduler.Instance;

        public IScheduler MainScheduler => this.mainScheduler;

        public IScheduler TaskPoolScheduler => Rx.TaskPoolScheduler.Default;
    }
}