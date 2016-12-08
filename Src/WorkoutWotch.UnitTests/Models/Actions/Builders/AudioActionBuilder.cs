namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using PCLMock;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;

    public sealed class AudioActionBuilder : IBuilder
    {
        private IAudioService audioService;
        private string audioName;

        public AudioActionBuilder()
        {
            this.audioService = new AudioServiceMock(MockBehavior.Loose);
            this.audioName = "name";
        }

        public AudioActionBuilder WithAudioService(IAudioService audioService) =>
            this.With(ref this.audioService, audioService);

        public AudioActionBuilder WithAudioName(string audioName) =>
            this.With(ref this.audioName, audioName);

        public AudioAction Build() =>
            new AudioAction(
                this.audioService,
                this.audioName);

        public static implicit operator AudioAction(AudioActionBuilder builder) =>
            builder.Build();
    }
}