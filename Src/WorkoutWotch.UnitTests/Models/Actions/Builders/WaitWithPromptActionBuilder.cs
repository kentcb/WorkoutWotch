namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    internal sealed class WaitWithPromptActionBuilder
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

        public WaitWithPromptAction Build()
        {
            return new WaitWithPromptAction(
                this.delayService,
                this.speechService,
                this.duration,
                this.promptSpeechText);
        }

        public WaitWithPromptActionBuilder WithDelayService(IDelayService delayService)
        {
            this.delayService = delayService;
            return this;
        }

        public WaitWithPromptActionBuilder WithSpeechService(ISpeechService speechService)
        {
            this.speechService = speechService;
            return this;
        }

        public WaitWithPromptActionBuilder WithDuration(TimeSpan duration)
        {
            this.duration = duration;
            return this;
        }

        public WaitWithPromptActionBuilder WithPromptSpeechText(string promptSpeechText)
        {
            this.promptSpeechText = promptSpeechText;
            return this;
        }
    }
}