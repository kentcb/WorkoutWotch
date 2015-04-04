namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class WaitAction : IAction
    {
        private static readonly TimeSpan maximumDelayTime = TimeSpan.FromSeconds(1);
        private readonly IDelayService delayService;
        private readonly TimeSpan delay;

        public WaitAction(IDelayService delayService, TimeSpan delay)
        {
            delayService.AssertNotNull(nameof(delayService));

            if (delay < TimeSpan.Zero)
            {
                throw new ArgumentException("delay must be greater than or equal to zero.", "delay");
            }

            this.delayService = delayService;
            this.delay = delay;
        }

        public TimeSpan Duration
        {
            get { return this.delay; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));

            var remainingDelay = this.delay;
            var skipAhead = MathExt.Min(remainingDelay, context.SkipAhead);

            if (skipAhead > TimeSpan.Zero)
            {
                remainingDelay = remainingDelay - skipAhead;
                context.AddProgress(skipAhead);
            }

            while (remainingDelay > TimeSpan.Zero)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                await context
                    .WaitWhilePausedAsync()
                    .ContinueOnAnyContext();

                var delayFor = MathExt.Min(remainingDelay, maximumDelayTime);

                await this.delayService
                    .DelayAsync(delayFor, context.CancellationToken)
                    .ContinueOnAnyContext();

                remainingDelay -= delayFor;
                context.AddProgress(delayFor);
            }
        }
    }
}