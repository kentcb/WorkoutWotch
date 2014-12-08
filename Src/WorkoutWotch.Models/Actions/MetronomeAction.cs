namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;

    public sealed class MetronomeAction : IAction
    {
        private readonly IAudioService audioService;
        private readonly IDelayService delayService;
        private readonly IImmutableList<MetronomeTick> ticks;
        private readonly TimeSpan duration;

        public MetronomeAction(IAudioService audioService, IDelayService delayService, IEnumerable<MetronomeTick> ticks)
        {
            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            ticks.AssertNotNull("ticks");

            this.audioService = audioService;
            this.delayService = delayService;
            this.ticks = ticks.ToImmutableList();
            this.duration = this.ticks
                .Select(x => x.PeriodBefore)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
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
            foreach (var tick in this.ticks)
            {
                yield return new WaitAction(this.delayService, tick.PeriodBefore);

                switch (tick.Type)
                {
                    case MetronomeTickType.Click:
                        yield return new AudioAction(this.audioService, "Audio/MetronomeClick.mp3");
                        break;
                    case MetronomeTickType.Bell:
                        yield return new AudioAction(this.audioService, "Audio/MetronomeBell.mp3");
                        break;
                }
            }
        }
    }
}