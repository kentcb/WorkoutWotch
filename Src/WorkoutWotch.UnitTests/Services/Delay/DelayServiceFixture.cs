namespace WorkoutWotch.UnitTests.Services.Delay
{
    using System;
    using System.Threading;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.UnitTests.Services.Delay.Builders;
    using Xunit;

    public sealed class DelayServiceFixture
    {
        [Fact]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new DelayServiceBuilder().WithSchedulerService(null).Build());
        }

        [Fact]
        public void delay_async_returns_observable_that_ticks_after_specified_delay()
        {
            var scheduler = new TestSchedulerService();
            var sut = new DelayServiceBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            var completed = false;
            sut
                .DelayAsync(TimeSpan.FromSeconds(5))
                .Subscribe(_ => completed = true);
            Assert.False(completed);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(1));
            Assert.False(completed);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
            Assert.False(completed);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(3));
            Assert.True(completed);
        }

        [Fact]
        public void delay_async_cancels_the_delay_if_cancellation_token_is_cancelled()
        {
            var scheduler = new TestSchedulerService();
            var sut = new DelayServiceBuilder()
                .WithSchedulerService(scheduler)
                .Build();
            var cts = new CancellationTokenSource();
            Exception exception = null;
            var delayResult = sut
                .DelayAsync(TimeSpan.FromSeconds(5), cts.Token)
                .Subscribe(
                    _ => { },
                    ex => exception = ex);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(1));
            Assert.Null(exception);

            cts.Cancel();
            scheduler.AdvanceBy(TimeSpan.FromSeconds(5));
            Assert.IsType<OperationCanceledException>(exception);
        }
    }
}