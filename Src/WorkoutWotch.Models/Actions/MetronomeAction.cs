namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class MetronomeAction : IAction
    {
        private readonly SequenceAction innerAction;

        public MetronomeAction(IAudioService audioService, IDelayService delayService, IEnumerable<MetronomeTick> ticks)
        {
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            ticks.AssertNotNull("ticks");

            this.innerAction = new SequenceAction(GetInnerActions(audioService, delayService, ticks));
        }

        public TimeSpan Duration
        {
            get { return this.innerAction.Duration; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");

            await this
                .innerAction
                .ExecuteAsync(context)
                .ContinueOnAnyContext();
        }

        private static IEnumerable<IAction> GetInnerActions(IAudioService audioService, IDelayService delayService, IEnumerable<MetronomeTick> ticks)
        {
            foreach (var tick in ticks)
            {
                yield return new WaitAction(delayService, tick.PeriodBefore);

                switch (tick.Type)
                {
                    case MetronomeTickType.Click:
                        yield return new AudioAction(audioService, "Audio/MetronomeClick.mp3");
                        break;
                    case MetronomeTickType.Bell:
                        yield return new AudioAction(audioService, "Audio/MetronomeBell.mp3");
                        break;
                }
            }
        }
    }
}