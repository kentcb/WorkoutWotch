namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class ExecutionContextFixture
    {
        [Test]
        public void cancel_cancels_the_cancellation_token()
        {
            var sut = new ExecutionContext();
            var token = sut.CancellationToken;
            Assert.False(sut.IsCancelled);
            Assert.False(token.IsCancellationRequested);

            sut.Cancel();
            Assert.True(sut.IsCancelled);
            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public void cancel_raises_property_changed_for_is_cancelled()
        {
            var sut = new ExecutionContext();
            var called = false;
            sut.ObservableForProperty(x => x.IsCancelled).Subscribe(_ => called = true);

            Assert.False(called);
            sut.Cancel();
            Assert.True(called);
        }

        [Test]
        public void wait_while_paused_async_completes_immediately_if_not_paused()
        {
            var sut = new ExecutionContext();
            var task = sut.WaitWhilePausedAsync();
            Assert.True(task.IsCompleted);
        }

        [Test]
        public void wait_while_paused_async_waits_until_unpaused()
        {
            var sut = new ExecutionContext();
            sut.IsPaused = true;
            var task = sut.WaitWhilePausedAsync();

            Assert.False(task.Wait(TimeSpan.FromMilliseconds(10)));
            sut.IsPaused = false;
            Assert.True(task.Wait(TimeSpan.FromMilliseconds(10)));
        }

        [Test]
        public void wait_while_paused_async_cancels_if_the_context_is_cancelled()
        {
            var sut = new ExecutionContext();
            sut.IsPaused = true;
            var task = sut.WaitWhilePausedAsync();

            Assert.False(task.Wait(TimeSpan.FromMilliseconds(10)));
            sut.Cancel();

            try
            {
                task.Wait(TimeSpan.FromMilliseconds(500));
                Assert.Fail("Expected task cancelled exception.");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.True(ex.InnerExceptions[0] is TaskCanceledException);
            }
        }

        [Test]
        public void add_progress_adds_to_the_progress()
        {
            var sut = new ExecutionContext();
            Assert.AreEqual(TimeSpan.Zero, sut.Progress);

            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), sut.Progress);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromMilliseconds(250), sut.Progress);

            sut.AddProgress(TimeSpan.FromMilliseconds(13));
            Assert.AreEqual(TimeSpan.FromMilliseconds(263), sut.Progress);
        }

        [Test]
        public void add_progress_adds_progress_to_the_current_exercise()
        {
            var sut = new ExecutionContext();
            Assert.AreEqual(TimeSpan.Zero, sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromMilliseconds(250), sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(13));
            Assert.AreEqual(TimeSpan.FromMilliseconds(263), sut.CurrentExerciseProgress);
        }

        [Test]
        public void add_progress_reduces_outstanding_skip_ahead()
        {
            var sut = new ExecutionContext(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromSeconds(2.9), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromSeconds(2.75), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(1000));
            Assert.AreEqual(TimeSpan.FromSeconds(1.75), sut.SkipAhead);
        }

        [Test]
        public void add_progress_does_not_reduce_skip_ahead_if_it_is_already_zero()
        {
            var sut = new ExecutionContext(TimeSpan.FromSeconds(1));
            Assert.AreEqual(TimeSpan.FromSeconds(1), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(900));
            Assert.AreEqual(TimeSpan.FromSeconds(0.1), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.Zero, sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(1000));
            Assert.AreEqual(TimeSpan.Zero, sut.SkipAhead);
        }

        [Test]
        public void setting_current_exercise_resets_the_current_exercise_progress_to_zero()
        {
            var sut = new ExecutionContext();
            sut.SetCurrentExercise(new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(MockBehavior.Loose), "name", 3, 10, Enumerable.Empty<MatcherWithAction>()));
            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), sut.CurrentExerciseProgress);

            sut.SetCurrentExercise(new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(MockBehavior.Loose), "name", 3, 10, Enumerable.Empty<MatcherWithAction>()));
            Assert.AreEqual(TimeSpan.Zero, sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromMilliseconds(150), sut.CurrentExerciseProgress);
        }
    }
}