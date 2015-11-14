namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
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
        public async Task execute_async_throws_if_context_is_null()
        {
            var sut = new AudioActionBuilder()
                .Build();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Fact]
        public async Task execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new AudioActionBuilder()
                .Build();

            using (var context = new ExecutionContext())
            {
                context.Cancel();

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.ExecuteAsync(context));
            }
        }

        [Fact]
        public async Task execute_async_pauses_if_context_is_paused()
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .Build();

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                await sut
                    .ExecuteAsync(context)
                    .Timeout(TimeSpan.FromMilliseconds(10))
                    .Catch(Observable.Return(Unit.Default));

                audioService
                    .Verify(x => x.PlayAsync(It.IsAny<string>()))
                    .WasNotCalled();
            }
        }

        [Theory]
        [InlineData("uri")]
        [InlineData("some_uri")]
        [InlineData("some/other/uri")]
        public async Task execute_async_passes_the_audio_resource_uri_onto_the_audio_service(string audioResourceUri)
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .WithAudioResourceUri(audioResourceUri)
                .Build();

            await sut.ExecuteAsync(new ExecutionContext());

            audioService
                .Verify(x => x.PlayAsync(audioResourceUri))
                .WasCalledExactlyOnce();
        }
    }
}