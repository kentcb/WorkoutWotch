namespace WorkoutWotch.Services.Scheduler
{
    using System.Reactive.Concurrency;
    using System.Threading;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using Rx = System.Reactive.Concurrency;

    public sealed class SchedulerService : ISchedulerService
    {
        private readonly IScheduler synchronizationContextScheduler;

        public SchedulerService()
        {
            this.synchronizationContextScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
        }

        public IScheduler DefaultScheduler => Rx.DefaultScheduler.Instance;

        public IScheduler CurrentThreadScheduler => Rx.CurrentThreadScheduler.Instance;

        public IScheduler ImmediateScheduler => Rx.ImmediateScheduler.Instance;

        public IScheduler SynchronizationContextScheduler => this.synchronizationContextScheduler;

        public IScheduler TaskPoolScheduler => Rx.TaskPoolScheduler.Default;
    }
}