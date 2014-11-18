namespace WorkoutWotch.Services.Scheduler
{
    using System.Reactive.Concurrency;
    using System.Threading;
    using WorkoutWotch.Services.Contracts.Scheduler;

    public sealed class SchedulerService : ISchedulerService
    {
        private readonly IScheduler synchronizationContextScheduler;

        public SchedulerService()
        {
            this.synchronizationContextScheduler = new System.Reactive.Concurrency.SynchronizationContextScheduler(SynchronizationContext.Current);
        }

        public IScheduler DefaultScheduler
        {
            get { return System.Reactive.Concurrency.DefaultScheduler.Instance; }
        }

        public IScheduler CurrentThreadScheduler
        {
            get { return System.Reactive.Concurrency.CurrentThreadScheduler.Instance; }
        }

        public IScheduler ImmediateScheduler
        {
            get { return System.Reactive.Concurrency.ImmediateScheduler.Instance; }
        }

        public IScheduler SynchronizationContextScheduler
        {
            get { return this.synchronizationContextScheduler; }
        }

        public IScheduler TaskPoolScheduler
        {
            get { return System.Reactive.Concurrency.TaskPoolScheduler.Default; }
        }
    }
}