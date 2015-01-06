namespace WorkoutWotch.UnitTests.Models.Actions
{
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    internal sealed class SayActionBuilder
    {
        private ISpeechService speechService;
        private string speechText;

        public SayActionBuilder()
        {
            this.speechService = new SpeechServiceMock(MockBehavior.Loose);
            this.speechText = "speech text";
        }

        public SayAction Build()
        {
            return new SayAction(
                this.speechService,
                this.speechText);
        }

        public SayActionBuilder WithSpeechService(ISpeechService speechService)
        {
            this.speechService = speechService;
            return this;
        }

        public SayActionBuilder WithSpeechText(string speechText)
        {
            this.speechText = speechText;
            return this;
        }
    }
}