namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System.Collections.Generic;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    internal sealed class MetronomeActionBuilder
    {
        private readonly IList<MetronomeTick> ticks;
        private IAudioService audioService;
        private IDelayService delayService;
        private ILoggerService loggerService;

        public MetronomeActionBuilder()
        {
            this.ticks = new List<MetronomeTick>();
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
        }

        public MetronomeAction Build()
        {
            return new MetronomeAction(
                this.audioService,
                this.delayService,
                this.loggerService,
                this.ticks);
        }

        public MetronomeActionBuilder WithAudioService(IAudioService audioService)
        {
            this.audioService = audioService;
            return this;
        }

        public MetronomeActionBuilder WithDelayService(IDelayService delayService)
        {
            this.delayService = delayService;
            return this;
        }

        public MetronomeActionBuilder WithLoggerService(ILoggerService loggerService)
        {
            this.loggerService = loggerService;
            return this;
        }

        public MetronomeActionBuilder AddMetronomeTick(MetronomeTick metronomeTick)
        {
            this.ticks.Add(metronomeTick);
            return this;
        }
    }
}