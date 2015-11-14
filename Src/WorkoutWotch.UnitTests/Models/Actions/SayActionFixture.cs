namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Builders;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using Xunit;

    public class SayActionFixture
    {
        [Fact]
        public void ctor_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SayAction(null, "something to say"));
        }

        [Fact]
        public void ctor_throws_if_speech_text_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SayAction(new SpeechServiceMock(), null));
        }

        [Fact]
        public void duration_returns_zero()
        {
            var sut = new SayActionBuilder()
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public async Task execute_async_throws_if_context_is_null()
        {
            var sut = new SayActionBuilder()
                .Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Fact]
        public async Task execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new SayActionBuilder()
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
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .Build();

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                await sut
                    .ExecuteAsync(context)
                    .Timeout(TimeSpan.FromMilliseconds(10))
                    .Catch(Observable.Return(Unit.Default));

                speechService
                    .Verify(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .WasNotCalled();
            }
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("hello, world")]
        [InlineData("you've got nothing to say. nothing but the one thing.")]
        public async Task execute_async_passes_the_speech_text_onto_the_speech_service(string speechText)
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .WithSpeechText(speechText)
                .Build();

            await sut.ExecuteAsync(new ExecutionContext());

            speechService
                .Verify(x => x.SpeakAsync(speechText, It.IsAny<CancellationToken>()))
                .WasCalledExactlyOnce();
        }
    }
}