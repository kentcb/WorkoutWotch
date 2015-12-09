namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using Builders;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using Xunit;

    public class AudioActionFixture
    {
        [Fact]
        public void ctor_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioAction(null, "some_uri"));
        }

        [Fact]
        public void ctor_throws_if_audio_resource_uri_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioAction(new AudioServiceMock(), null));
        }

        [Fact]
        public void duration_returns_zero()
        {
            var sut = new AudioActionBuilder()
                .Build();
            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new AudioActionBuilder()
                .Build();
            Assert.Throws<ArgumentNullException>(() => sut.ExecuteAsync(null));
        }

        [Fact]
        public void execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new AudioActionBuilder()
                .Build();

            using (var context = new ExecutionContext())
            {
                context.Cancel();

                Assert.Throws<OperationCanceledException>(() => sut.ExecuteAsync(context));
            }
        }

        [Fact]
        public void execute_async_pauses_if_context_is_paused()
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .Build();

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                sut.ExecuteAsync(context);

                audioService
                    .Verify(x => x.PlayAsync(It.IsAny<string>()))
                    .WasNotCalled();
            }
        }

        [Theory]
        [InlineData("name")]
        [InlineData("some_name")]
        [InlineData("some_other_name")]
        public void execute_async_passes_the_audio_name_onto_the_audio_service(string audioName)
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .WithAudioName(audioName)
                .Build();

            sut.ExecuteAsync(new ExecutionContext());

            audioService
                .Verify(x => x.PlayAsync(audioName))
                .WasCalledExactlyOnce();
        }
    }
}