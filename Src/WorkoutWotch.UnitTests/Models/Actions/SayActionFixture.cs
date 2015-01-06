namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class SayActionFixture
    {
        [Test]
        public void ctor_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SayAction(null, "something to say"));
        }

        [Test]
        public void ctor_throws_if_speech_text_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SayAction(new SpeechServiceMock(), null));
        }

        [Test]
        public void duration_returns_zero()
        {
            var sut = new SayActionBuilder().Build();

            Assert.AreEqual(TimeSpan.Zero, sut.Duration);
        }

        [Test]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new SayActionBuilder().Build();

            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public void execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new SayActionBuilder().Build();

            using (var context = new ExecutionContext())
            {
                context.Cancel();

                Assert.Throws<OperationCanceledException>(async () => await sut.ExecuteAsync(context));
            }
        }

        [Test]
        public void execute_async_pauses_if_context_is_paused()
        {
            var sut = new SayActionBuilder().Build();

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                var task = sut.ExecuteAsync(context);

                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));
            }
        }

        [TestCase("hello")]
        [TestCase("hello, world")]
        [TestCase("you've got nothing to say. nothing but the one thing.")]
        public async Task execute_async_passes_the_speech_text_onto_the_speech_service(string speechText)
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayActionBuilder()
                .WithSpeechService(speechService)
                .WithSpeechText(speechText)
                .Build();

            await sut.ExecuteAsync(new ExecutionContext());

            speechService
                .Verify(x => x.SpeakAsync(It.Is(speechText), It.IsAny<CancellationToken>()))
                .WasCalledExactlyOnce();
        }
    }
}