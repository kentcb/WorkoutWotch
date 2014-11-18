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

        public IScheduler DefaultScheduler
        {
            get { return Rx.DefaultScheduler.Instance; }
        }

        public IScheduler CurrentThreadScheduler
        {
            get { return Rx.CurrentThreadScheduler.Instance; }
        }

        public IScheduler ImmediateScheduler
        {
            get { return Rx.ImmediateScheduler.Instance; }
        }

        public IScheduler SynchronizationContextScheduler
        {
            get { return this.synchronizationContextScheduler; }
        }

        public IScheduler TaskPoolScheduler
        {
            get { return Rx.TaskPoolScheduler.Default; }
        }
    }
}