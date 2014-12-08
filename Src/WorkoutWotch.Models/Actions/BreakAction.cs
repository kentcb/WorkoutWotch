namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class BreakAction : IAction
    {
        private static readonly TimeSpan minimumBreakToIncludeReady = TimeSpan.FromSeconds(2);
        private readonly IDelayService delayService;
        private readonly ISpeechService speechService;
        private readonly TimeSpan duration;

        public BreakAction(IDelayService delayService, ISpeechService speechService, TimeSpan duration)
        {
            delayService.AssertNotNull("delayService");
            speechService.AssertNotNull("speechService");

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentException("duration must be greater than or equal to zero.", "duration");
            }

            this.delayService = delayService;
            this.speechService = speechService;
            this.duration = duration;
        }

        public TimeSpan Duration
        {
            get { return this.duration; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");

            foreach (var innerAction in this.GetInnerActions())
            {
                await innerAction
                    .ExecuteAsync(context)
                    .ContinueOnAnyContext();
            }
        }

        private IEnumerable<IAction> GetInnerActions()
        {
            yield return new SayAction(this.speechService, "break");

            if (this.duration < minimumBreakToIncludeReady)
            {
                yield return new WaitAction(this.delayService, this.duration);
            }
            else
            {
                yield return new WaitAction(this.delayService, this.duration - minimumBreakToIncludeReady);
                yield return new SayAction(this.speechService, "ready?");
                yield return new WaitAction(this.delayService, minimumBreakToIncludeReady);
            }
        }
    }
}