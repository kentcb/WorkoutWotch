namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class WaitWithPromptAction : IAction
    {
        private static readonly TimeSpan minimumBreakToIncludeReady = TimeSpan.FromSeconds(2);
        private readonly SequenceAction innerAction;

        public WaitWithPromptAction(IDelayService delayService, ISpeechService speechService, TimeSpan duration, string promptSpeechText)
        {
            delayService.AssertNotNull(nameof(delayService));
            speechService.AssertNotNull(nameof(speechService));
            promptSpeechText.AssertNotNull(nameof(promptSpeechText));

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentException("duration must be greater than or equal to zero.", "duration");
            }

            this.innerAction = new SequenceAction(GetInnerActions(delayService, speechService, duration, promptSpeechText));
        }

        public TimeSpan Duration
        {
            get { return this.innerAction.Duration; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));

            await this
                .innerAction
                .ExecuteAsync(context)
                .ContinueOnAnyContext();
        }

        private static IEnumerable<IAction> GetInnerActions(IDelayService delayService, ISpeechService speechService, TimeSpan duration, string promptSpeechText)
        {
            yield return new SayAction(speechService, promptSpeechText);

            if (duration < minimumBreakToIncludeReady)
            {
                yield return new WaitAction(delayService, duration);
            }
            else
            {
                yield return new WaitAction(delayService, duration - minimumBreakToIncludeReady);
                yield return new SayAction(speechService, "ready?");
                yield return new WaitAction(delayService, minimumBreakToIncludeReady);
            }
        }
    }
}