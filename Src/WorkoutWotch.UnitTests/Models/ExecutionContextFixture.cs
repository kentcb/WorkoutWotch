namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Builders;
    using global::ReactiveUI;
    using WorkoutWotch.Models;
    using Xunit;

    public class ExecutionContextFixture
    {
        [Fact]
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

        [Fact]
        public void cancel_raises_property_changed_for_is_cancelled()
        {
            var sut = new ExecutionContext();
            var called = false;
            sut.ObservableForProperty(x => x.IsCancelled)
                .Subscribe(_ => called = true);

            Assert.False(called);
            sut.Cancel();
            Assert.True(called);
        }

        [Fact]
        public void wait_while_paused_async_completes_immediately_if_not_paused()
        {
            var sut = new ExecutionContext();
            var completed = false;
            sut
                .WaitWhilePausedAsync()
                .Subscribe(_ => completed = true);
            Assert.True(completed);
        }

        [Fact]
        public async Task wait_while_paused_async_waits_until_unpaused()
        {
            var sut = new ExecutionContext();
            sut.IsPaused = true;

            await Assert.ThrowsAsync<TimeoutException>(
                async () =>
                    await sut
                        .WaitWhilePausedAsync()
                        .Timeout(TimeSpan.FromMilliseconds(10)));

            sut.IsPaused = false;

            await sut
                .WaitWhilePausedAsync()
                .TimeoutIfTooSlow();
        }

        [Fact]
        public async Task wait_while_paused_async_cancels_if_the_context_is_cancelled()
        {
            var sut = new ExecutionContext();
            sut.IsPaused = true;

            await Assert.ThrowsAsync<TimeoutException>(
                async () =>
                    await sut
                        .WaitWhilePausedAsync()
                        .Timeout(TimeSpan.FromMilliseconds(10)));

            sut.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                async () =>
                    await sut
                        .WaitWhilePausedAsync()
                        .TimeoutIfTooSlow());
        }

        [Fact]
        public void add_progress_adds_to_the_progress()
        {
            var sut = new ExecutionContext();
            Assert.Equal(TimeSpan.Zero, sut.Progress);

            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.Equal(TimeSpan.FromMilliseconds(100), sut.Progress);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.Equal(TimeSpan.FromMilliseconds(250), sut.Progress);

            sut.AddProgress(TimeSpan.FromMilliseconds(13));
            Assert.Equal(TimeSpan.FromMilliseconds(263), sut.Progress);
        }

        [Fact]
        public void add_progress_adds_progress_to_the_current_exercise()
        {
            var sut = new ExecutionContext();
            Assert.Equal(TimeSpan.Zero, sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.Equal(TimeSpan.FromMilliseconds(100), sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.Equal(TimeSpan.FromMilliseconds(250), sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(13));
            Assert.Equal(TimeSpan.FromMilliseconds(263), sut.CurrentExerciseProgress);
        }

        [Fact]
        public void add_progress_reduces_outstanding_skip_ahead()
        {
            var sut = new ExecutionContext(TimeSpan.FromSeconds(3));
            Assert.Equal(TimeSpan.FromSeconds(3), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.Equal(TimeSpan.FromSeconds(2.9), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.Equal(TimeSpan.FromSeconds(2.75), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(1000));
            Assert.Equal(TimeSpan.FromSeconds(1.75), sut.SkipAhead);
        }

        [Fact]
        public void add_progress_does_not_reduce_skip_ahead_if_it_is_already_zero()
        {
            var sut = new ExecutionContext(TimeSpan.FromSeconds(1));
            Assert.Equal(TimeSpan.FromSeconds(1), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(900));
            Assert.Equal(TimeSpan.FromSeconds(0.1), sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.Equal(TimeSpan.Zero, sut.SkipAhead);

            sut.AddProgress(TimeSpan.FromMilliseconds(1000));
            Assert.Equal(TimeSpan.Zero, sut.SkipAhead);
        }

        [Fact]
        public void setting_current_exercise_resets_the_current_exercise_progress_to_zero()
        {
            var sut = new ExecutionContext();

            sut.SetCurrentExercise(new ExerciseBuilder()
                .WithSetCount(3)
                .WithRepetitionCount(10)
                .Build());
            sut.AddProgress(TimeSpan.FromMilliseconds(100));
            Assert.Equal(TimeSpan.FromMilliseconds(100), sut.CurrentExerciseProgress);

            sut.SetCurrentExercise(new ExerciseBuilder()
                .WithSetCount(3)
                .WithRepetitionCount(10)
                .Build());
            Assert.Equal(TimeSpan.Zero, sut.CurrentExerciseProgress);

            sut.AddProgress(TimeSpan.FromMilliseconds(150));
            Assert.Equal(TimeSpan.FromMilliseconds(150), sut.CurrentExerciseProgress);
        }
    }
}