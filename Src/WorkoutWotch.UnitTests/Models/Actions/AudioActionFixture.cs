namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using Xunit;

    public sealed class AudioActionFixture
    {
        [Fact]
        public void duration_returns_zero()
        {
            var sut = new AudioActionBuilder()
                .Build();
            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void execute_pauses_if_context_is_paused()
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .Build();
            var context = new ExecutionContext();

            context.IsPaused = true;

            sut
                .Execute(context)
                .Subscribe();

            audioService
                .Verify(x => x.Play(It.IsAny<string>()))
                .WasNotCalled();
        }

        [Theory]
        [InlineData("name")]
        [InlineData("some_name")]
        [InlineData("some_other_name")]
        public void execute_passes_the_audio_name_onto_the_audio_service(string audioName)
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .WithAudioName(audioName)
                .Build();

            sut
                .Execute(new ExecutionContext())
                .Subscribe();

            audioService
                .Verify(x => x.Play(audioName))
                .WasCalledExactlyOnce();
        }
    }
}