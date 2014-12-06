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
            Assert.AreEqual(TimeSpan.Zero, new SayAction(new SpeechServiceMock(), "something to say").Duration);
        }

        [Test]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new SayAction(new SpeechServiceMock(), "something to say");
            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new SayAction(new SpeechServiceMock(), "something to say");

            using (var context = new ExecutionContext())
            {
                context.Cancel();

                try
                {
                    await sut.ExecuteAsync(context);
                    Assert.Fail("Expecting operation canceled exception.");
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        [Test]
        public void execute_async_pauses_if_context_is_paused()
        {
            var sut = new SayAction(new SpeechServiceMock(), "something to say");

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                var task = sut.ExecuteAsync(context);

                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));
            }
        }

        [Test]
        public async Task execute_async_passes_the_speech_text_onto_the_speech_service()
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);
            var sut = new SayAction(speechService, "something to say");

            await sut.ExecuteAsync(new ExecutionContext());

            speechService.Verify(x => x.SpeakAsync(It.Is("something to say"), It.IsAny<CancellationToken>())).WasCalledExactlyOnce();
        }
    }
}