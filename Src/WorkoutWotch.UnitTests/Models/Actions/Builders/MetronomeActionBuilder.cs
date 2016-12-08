namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System.Collections.Generic;
    using PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    public sealed class MetronomeActionBuilder : IBuilder
    {
        private IAudioService audioService;
        private IDelayService delayService;
        private ILoggerService loggerService;
        private List<MetronomeTick> ticks;

        public MetronomeActionBuilder()
        {
            this.ticks = new List<MetronomeTick>();
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
        }

        public MetronomeActionBuilder WithAudioService(IAudioService audioService) =>
            this.With(ref this.audioService, audioService);

        public MetronomeActionBuilder WithDelayService(IDelayService delayService) =>
            this.With(ref this.delayService, delayService);

        public MetronomeActionBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public MetronomeActionBuilder WithMetronomeTick(MetronomeTick metronomeTick) =>
            this.With(ref this.ticks, metronomeTick);

        public MetronomeActionBuilder WithMetronomeTicks(IEnumerable<MetronomeTick> metronomeTicks) =>
            this.With(ref this.ticks, metronomeTicks);

        public MetronomeAction Build() =>
            new MetronomeAction(
                this.audioService,
                this.delayService,
                this.loggerService,
                this.ticks);

        public static implicit operator MetronomeAction(MetronomeActionBuilder builder) =>
            builder.Build();
    }
}