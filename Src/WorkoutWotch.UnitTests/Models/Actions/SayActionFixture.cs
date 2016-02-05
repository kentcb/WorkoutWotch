namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Threading;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using Xunit;

    public class SayActionFixture
    {
        [Fact]
        public void duration_returns_zero()
        {
            var sut = new SayActionBuilder()
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new SayActionBuilder()
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
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .Build();

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                sut.ExecuteAsync(context);

                speechService
                    .Verify(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .WasNotCalled();
            }
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("hello, world")]
        [InlineData("you've got nothing to say. nothing but the one thing.")]
        public void execute_async_passes_the_speech_text_onto_the_speech_service(string speechText)
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .WithSpeechText(speechText)
                .Build();

            sut.ExecuteAsync(new ExecutionContext());

            speechService
                .Verify(x => x.SpeakAsync(speechText, It.IsAny<CancellationToken>()))
                .WasCalledExactlyOnce();
        }
    }
}