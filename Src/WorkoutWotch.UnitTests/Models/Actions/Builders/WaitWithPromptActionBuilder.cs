namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System;
    using Genesis.TestUtil;
    using PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    public sealed class WaitWithPromptActionBuilder : IBuilder
    {
        private IDelayService delayService;
        private ISpeechService speechService;
        private TimeSpan duration;
        private string promptSpeechText;

        public WaitWithPromptActionBuilder()
        {
            this.delayService = new DelayServiceMock(MockBehavior.Loose);
            this.speechService = new SpeechServiceMock(MockBehavior.Loose);
            this.promptSpeechText = "prompt";
        }

        public WaitWithPromptActionBuilder WithDelayService(IDelayService delayService) =>
            this.With(ref this.delayService, delayService);

        public WaitWithPromptActionBuilder WithSpeechService(ISpeechService speechService) =>
            this.With(ref this.speechService, speechService);

        public WaitWithPromptActionBuilder WithDuration(TimeSpan duration) =>
            this.With(ref this.duration, duration);

        public WaitWithPromptActionBuilder WithPromptSpeechText(string promptSpeechText) =>
            this.With(ref this.promptSpeechText, promptSpeechText);

        public WaitWithPromptAction Build() =>
            new WaitWithPromptAction(
                this.delayService,
                this.speechService,
                this.duration,
                this.promptSpeechText);

        public static implicit operator WaitWithPromptAction(WaitWithPromptActionBuilder builder) =>
            builder.Build();
    }
}