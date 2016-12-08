namespace WorkoutWotch.UnitTests.Services.Delay
{
    using System;
    using Microsoft.Reactive.Testing;
    using WorkoutWotch.UnitTests.Services.Delay.Builders;
    using Xunit;

    public sealed class DelayServiceFixture
    {
        [Fact]
        public void delay_returns_observable_that_ticks_after_specified_delay()
        {
            var scheduler = new TestScheduler();
            var sut = new DelayServiceBuilder()
                .WithScheduler(scheduler)
                .Build();

            var completed = false;
            sut
                .Delay(TimeSpan.FromSeconds(5))
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
        public void delay_cancels_the_delay_if_subscription_is_disposed()
        {
            var scheduler = new TestScheduler();
            var sut = new DelayServiceBuilder()
                .WithScheduler(scheduler)
                .Build();
            var executed = false;
            var delayResult = sut
                .Delay(TimeSpan.FromSeconds(5))
                .Subscribe(_ => executed = true);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(1));
            Assert.False(executed);

            delayResult.Dispose();
            scheduler.AdvanceBy(TimeSpan.FromSeconds(5));
            Assert.False(executed);
        }
    }
}