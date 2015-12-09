namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;

    internal sealed class AudioActionBuilder
    {
        private IAudioService audioService;
        private string audioName;

        public AudioActionBuilder()
        {
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.audioName = "name";
        }

        public AudioActionBuilder WithAudioService(IAudioService audioService)
        {
            this.audioService = audioService;
            return this;
        }

        public AudioActionBuilder WithAudioName(string audioName)
        {
            this.audioName = audioName;
            return this;
        }

        public AudioAction Build() =>
            new AudioAction(
                this.audioService,
                this.audioName);

        public static implicit operator AudioAction(AudioActionBuilder builder) =>
            builder.Build();
    }
}