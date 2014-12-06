using System;
using NUnit.Framework;
using WorkoutWotch.Models.Actions;
using WorkoutWotch.UnitTests.Services.Audio.Mocks;
using System.Threading.Tasks;
using WorkoutWotch.Models;
using Kent.Boogaart.PCLMock;
using System.Threading;

namespace WorkoutWotch.UnitTests.Models.Actions
{
    [TestFixture]
    public class AudioActionFixture
    {
        [Test]
        public void ctor_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioAction(null, "some_uri"));
        }

        [Test]
        public void ctor_throws_if_audio_resource_uri_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioAction(new AudioServiceMock(), null));
        }

        [Test]
        public void duration_returns_zero()
        {
            Assert.AreEqual(TimeSpan.Zero, new AudioAction(new AudioServiceMock(), "some_uri").Duration);
        }

        [Test]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new AudioAction(new AudioServiceMock(), "some_uri");
            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new AudioAction(new AudioServiceMock(), "some_uri");

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
            var sut = new AudioAction(new AudioServiceMock(), "some_uri");

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                var task = sut.ExecuteAsync(context);

                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));
            }
        }

        [Test]
        public async Task execute_async_passes_the_audio_resource_uri_onto_the_audio_service()
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioAction(audioService, "some_uri");

            await sut.ExecuteAsync(new ExecutionContext());

            audioService.Verify(x => x.PlayAsync(It.Is("some_uri"))).WasCalledExactlyOnce();
        }
    }
}

