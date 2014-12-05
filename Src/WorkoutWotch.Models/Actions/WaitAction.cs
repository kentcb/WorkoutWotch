using System;
using System.Threading.Tasks;
using WorkoutWotch.Services.Contracts.Delay;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Actions
{
    public sealed class WaitAction : IAction
    {
        private static readonly TimeSpan maximumDelayTime = TimeSpan.FromSeconds(1);
        private readonly IDelayService delayService;
        private readonly TimeSpan delay;

        public WaitAction(IDelayService delayService, TimeSpan delay)
        {
            delayService.AssertNotNull("delayService");

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
            context.AssertNotNull("context");

            var remainingDelay = this.delay;

            if (context.SkipAhead > TimeSpan.Zero)
            {
                remainingDelay = remainingDelay - context.SkipAhead;

                if (remainingDelay < TimeSpan.Zero)
                {
                    remainingDelay = TimeSpan.Zero;
                }

                context.AddProgress(context.SkipAhead);
            }

            while (remainingDelay > TimeSpan.Zero)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await context.WaitWhilePausedAsync().ContinueOnAnyContext();

                var delayFor = remainingDelay > maximumDelayTime ? maximumDelayTime : remainingDelay;
                await this.delayService.DelayAsync(delayFor, context.CancellationToken).ContinueOnAnyContext();

                remainingDelay -= delayFor;
                context.AddProgress(delayFor);
            }
        }
    }
}

