namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using ReactiveUI;
    using WorkoutWotch.Models;

    [TestFixture]
    public class ExecutionContextFixture
    {
        [Test]
        public void cancel_cancels_the_cancellation_token()
        {
            var context = new ExecutionContext();
            var token = context.CancellationToken;
            Assert.False(context.IsCancelled);
            Assert.False(token.IsCancellationRequested);

            context.Cancel();
            Assert.True(context.IsCancelled);
            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public void cancel_raises_property_changed_for_is_cancelled()
        {
            var context = new ExecutionContext();
            var called = false;
            context.ObservableForProperty(x => x.IsCancelled).Subscribe(_ => called = true);

            Assert.False(called);
            context.Cancel();
            Assert.True(called);
        }

        [Test]
        public void wait_while_paused_async_completes_immediately_if_not_paused()
        {
            var context = new ExecutionContext();
            var task = context.WaitWhilePausedAsync();
            Assert.True(task.IsCompleted);
        }

        [Test]
        public void wait_while_paused_async_waits_until_unpaused()
        {
            var context = new ExecutionContext();
            context.IsPaused = true;
            var task = context.WaitWhilePausedAsync();

            Assert.False(task.Wait(TimeSpan.FromMilliseconds(10)));
            context.IsPaused = false;
            Assert.True(task.Wait(TimeSpan.FromMilliseconds(10)));
        }

        [Test]
        public void wait_while_paused_async_cancels_if_the_context_is_cancelled()
        {
            var context = new ExecutionContext();
            context.IsPaused = true;
            var task = context.WaitWhilePausedAsync();

            Assert.False(task.Wait(TimeSpan.FromMilliseconds(10)));
            context.Cancel();

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
            var context = new ExecutionContext();
            Assert.AreEqual(TimeSpan.Zero, context.Progress);

            context.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), context.Progress);

            context.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromMilliseconds(250), context.Progress);

            context.AddProgress(TimeSpan.FromMilliseconds(13));
            Assert.AreEqual(TimeSpan.FromMilliseconds(263), context.Progress);
        }

        [Test]
        public void add_progress_adds_progress_to_the_current_exercise()
        {
            var context = new ExecutionContext();
            Assert.AreEqual(TimeSpan.Zero, context.CurrentExerciseProgress);

            context.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), context.CurrentExerciseProgress);

            context.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromMilliseconds(250), context.CurrentExerciseProgress);

            context.AddProgress(TimeSpan.FromMilliseconds(13));
            Assert.AreEqual(TimeSpan.FromMilliseconds(263), context.CurrentExerciseProgress);
        }

        [Test]
        public void add_progress_reduces_outstanding_skip_ahead()
        {
            var context = new ExecutionContext(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), context.SkipAhead);

            context.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromSeconds(2.9), context.SkipAhead);

            context.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromSeconds(2.75), context.SkipAhead);

            context.AddProgress(TimeSpan.FromMilliseconds(1000));
            Assert.AreEqual(TimeSpan.FromSeconds(1.75), context.SkipAhead);
        }

        [Test]
        public void add_progress_does_not_reduce_skip_ahead_if_it_is_already_zero()
        {
            var context = new ExecutionContext(TimeSpan.FromSeconds(1));
            Assert.AreEqual(TimeSpan.FromSeconds(1), context.SkipAhead);

            context.AddProgress(TimeSpan.FromMilliseconds(900));
            Assert.AreEqual(TimeSpan.FromSeconds(0.1), context.SkipAhead);

            context.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.Zero, context.SkipAhead);

            context.AddProgress(TimeSpan.FromMilliseconds(1000));
            Assert.AreEqual(TimeSpan.Zero, context.SkipAhead);
        }

        [Test]
        public void setting_current_exercise_resets_the_current_exercise_progress_to_zero()
        {
            var context = new ExecutionContext();
            context.SetCurrentExercise(new Exercise());
            context.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), context.CurrentExerciseProgress);

            context.SetCurrentExercise(new Exercise());
            Assert.AreEqual(TimeSpan.Zero, context.CurrentExerciseProgress);

            context.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.AreEqual(TimeSpan.FromMilliseconds(150), context.CurrentExerciseProgress);
        }
    }
}