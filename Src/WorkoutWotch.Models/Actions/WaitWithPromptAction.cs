namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class WaitWithPromptAction : IAction
    {
        private static readonly TimeSpan minimumBreakToIncludeReady = TimeSpan.FromSeconds(2);
        private readonly SequenceAction innerAction;

        public WaitWithPromptAction(IDelayService delayService, ISpeechService speechService, TimeSpan duration, string promptSpeechText)
        {
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));
            Ensure.ArgumentNotNull(promptSpeechText, nameof(promptSpeechText));
            Ensure.ArgumentCondition(duration >= TimeSpan.Zero, "duration must be greater than or equal to zero.", nameof(duration));

            this.innerAction = new SequenceAction(GetInnerActions(delayService, speechService, duration, promptSpeechText));
        }

        public TimeSpan Duration => this.innerAction.Duration;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            return this
                .innerAction
                .Execute(context);
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