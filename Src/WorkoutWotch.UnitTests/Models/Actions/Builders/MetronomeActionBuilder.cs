namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System.Collections.Generic;
    using Genesis.TestUtil;
    using PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;

    public sealed class MetronomeActionBuilder : IBuilder
    {
        private IAudioService audioService;
        private IDelayService delayService;
        private List<MetronomeTick> ticks;

        public MetronomeActionBuilder()
        {
            this.ticks = new List<MetronomeTick>();
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
        }

        public MetronomeActionBuilder WithAudioService(IAudioService audioService) =>
            this.With(ref this.audioService, audioService);

        public MetronomeActionBuilder WithDelayService(IDelayService delayService) =>
            this.With(ref this.delayService, delayService);

        public MetronomeActionBuilder WithMetronomeTick(MetronomeTick metronomeTick) =>
            this.With(ref this.ticks, metronomeTick);

        public MetronomeActionBuilder WithMetronomeTicks(IEnumerable<MetronomeTick> metronomeTicks) =>
            this.With(ref this.ticks, metronomeTicks);

        public MetronomeAction Build() =>
            new MetronomeAction(
                this.audioService,
                this.delayService,
                this.ticks);

        public static implicit operator MetronomeAction(MetronomeActionBuilder builder) =>
            builder.Build();
    }
}