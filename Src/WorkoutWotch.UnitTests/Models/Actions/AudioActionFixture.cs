namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;

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
            var sut = new AudioActionBuilder().Build();
            Assert.AreEqual(TimeSpan.Zero, sut.Duration);
        }

        [Test]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new AudioActionBuilder().Build();
            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public void execute_async_cancels_if_context_is_cancelled()
        {
            var sut = new AudioActionBuilder().Build();

            using (var context = new ExecutionContext())
            {
                context.Cancel();

                Assert.Throws<OperationCanceledException>(async () => await sut.ExecuteAsync(context));
            }
        }

        [Test]
        public void execute_async_pauses_if_context_is_paused()
        {
            var sut = new AudioActionBuilder().Build();

            using (var context = new ExecutionContext())
            {
                context.IsPaused = true;

                var task = sut.ExecuteAsync(context);

                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));
            }
        }

        [TestCase("uri")]
        [TestCase("some_uri")]
        [TestCase("some/other/uri")]
        public async Task execute_async_passes_the_audio_resource_uri_onto_the_audio_service(string audioResourceUri)
        {
            var audioService = new AudioServiceMock(MockBehavior.Loose);
            var sut = new AudioActionBuilder()
                .WithAudioService(audioService)
                .WithAudioResourceUri(audioResourceUri)
                .Build();

            await sut.ExecuteAsync(new ExecutionContext());

            audioService
                .Verify(x => x.PlayAsync(It.Is(audioResourceUri)))
                .WasCalledExactlyOnce();
        }
    }
}