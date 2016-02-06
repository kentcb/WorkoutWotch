namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    internal sealed class SayActionBuilder : IBuilder
    {
        private ISpeechService speechService;
        private string speechText;

        public SayActionBuilder()
        {
            this.speechService = new SpeechServiceMock(MockBehavior.Loose);
            this.speechText = "speech text";
        }

        public SayActionBuilder WithSpeechService(ISpeechService speechService) =>
            this.With(ref this.speechService, speechService);

        public SayActionBuilder WithSpeechText(string speechText) =>
            this.With(ref this.speechText, speechText);

        public SayAction Build() =>
            new SayAction(
                this.speechService,
                this.speechText);
        
        public static implicit operator SayAction(SayActionBuilder builder) =>
            builder.Build();
    }
}