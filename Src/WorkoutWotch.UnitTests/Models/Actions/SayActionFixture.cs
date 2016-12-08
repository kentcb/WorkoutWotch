namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using Xunit;

    public sealed class SayActionFixture
    {
        [Fact]
        public void duration_returns_zero()
        {
            var sut = new SayActionBuilder()
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void execute_pauses_if_context_is_paused()
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .Build();
            var context = new ExecutionContext();

            context.IsPaused = true;

            sut.Execute(context).Subscribe();

            speechService
                .Verify(x => x.Speak(It.IsAny<string>()))
                .WasNotCalled();
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("hello, world")]
        [InlineData("you've got nothing to say. nothing but the one thing.")]
        public void execute_passes_the_speech_text_onto_the_speech_service(string speechText)
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .WithSpeechText(speechText)
                .Build();

            sut.Execute(new ExecutionContext()).Subscribe();

            speechService
                .Verify(x => x.Speak(speechText))
                .WasCalledExactlyOnce();
        }
    }
}