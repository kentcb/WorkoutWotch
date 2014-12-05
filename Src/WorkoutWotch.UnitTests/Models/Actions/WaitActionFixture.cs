namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;

    [TestFixture]
    public class WaitActionFixture
    {
        [Test]
        public void ctor_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitAction(null, TimeSpan.Zero));
        }

        [Test]
        public void ctor_throws_if_delay_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => new WaitAction(new DelayServiceMock(), TimeSpan.FromSeconds(-1)));
        }

        [Test]
        public void duration_returns_delay()
        {
            var sut = new WaitAction(new DelayServiceMock(), TimeSpan.FromSeconds(23));
            Assert.AreEqual(TimeSpan.FromSeconds(23), sut.Duration);
        }

        [Test]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new WaitAction(new DelayServiceMock(), TimeSpan.Zero);
            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_breaks_for_the_specified_delay()
        {
            var delayService = new DelayServiceMock();
            var totalDelay = TimeSpan.Zero;
            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((t, ct) => totalDelay += t)
                .Return(Task.FromResult(true));
            var sut = new WaitAction(delayService, TimeSpan.FromMilliseconds(850));

            await sut.ExecuteAsync(new ExecutionContext());

            Assert.AreEqual(TimeSpan.FromMilliseconds(850), totalDelay);
        }

        [Test]
        public async Task execute_async_skips_ahead_if_the_context_has_skip_ahead()
        {
            var delayService = new DelayServiceMock();
            var totalDelay = TimeSpan.Zero;
            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((t, ct) => totalDelay += t)
                .Return(Task.FromResult(true));
            var sut = new WaitAction(delayService, TimeSpan.FromMilliseconds(850));

            await sut.ExecuteAsync(new ExecutionContext(TimeSpan.FromMilliseconds(800)));

            Assert.AreEqual(TimeSpan.FromMilliseconds(50), totalDelay);
        }

        [Test]
        public async Task execute_async_skips_ahead_if_the_context_has_skip_ahead_even_if_the_context_is_paused()
        {
            var delayService = new DelayServiceMock();
            var totalDelay = TimeSpan.Zero;
            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((t, ct) => totalDelay += t)
                .Return(Task.FromResult(true));
            var sut = new WaitAction(delayService, TimeSpan.FromMilliseconds(850));

            using (var context = new ExecutionContext(TimeSpan.FromMilliseconds(800)) { IsPaused = true })
            {
                var task = sut.ExecuteAsync(context);

                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));

                await context
                    .WhenAnyValue(x => x.Progress)
                    .Where(x => x == TimeSpan.FromMilliseconds(800))
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3))
                    .ToTask();
            }
        }

        [Test]
        public async Task execute_async_reports_progress()
        {
            var delayService = new DelayServiceMock(MockBehavior.Loose);
            var sut = new WaitAction(delayService, TimeSpan.FromMilliseconds(50));

            using (var context = new ExecutionContext())
            {
                Assert.AreEqual(TimeSpan.Zero, context.Progress);

                await sut.ExecuteAsync(context);

                Assert.AreEqual(TimeSpan.FromMilliseconds(50), context.Progress);
            }
        }

        [Test]
        public async Task execute_async_reports_progress_correctly_even_if_the_skip_ahead_exceeds_the_wait_duration()
        {
            var delayService = new DelayServiceMock(MockBehavior.Loose);
            var sut = new WaitAction(delayService, TimeSpan.FromMilliseconds(50));

            using (var context = new ExecutionContext(TimeSpan.FromMilliseconds(100)))
            {
                Assert.AreEqual(TimeSpan.Zero, context.Progress);

                await sut.ExecuteAsync(context);

                Assert.AreEqual(TimeSpan.FromMilliseconds(50), context.Progress);
            }
        }

        [Test]
        public async Task execute_async_bails_out_if_context_is_cancelled()
        {
            var delayService = new DelayServiceMock();
            var delayCallCount = 0;

            using (var context = new ExecutionContext())
            {
                delayService
                    .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .Do(
                        () =>
                        {
                            if (delayCallCount++ == 2)
                            {
                                context.Cancel();
                            }
                        })
                    .Return(Task.FromResult(true));

                var sut = new WaitAction(delayService, TimeSpan.FromSeconds(50));

                try
                {
                    await sut.ExecuteAsync(context);
                    Assert.Fail("Expecting operation cancelled exception.");
                }
                catch (OperationCanceledException)
                {
                }

                Assert.True(context.IsCancelled);
            }
        }

        [Test]
        public async Task execute_async_pauses_if_context_is_paused()
        {
            var delayService = new DelayServiceMock();
            var delayCallCount = 0;

            using (var context = new ExecutionContext())
            {
                delayService
                    .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .Do(
                        () =>
                        {
                            if (delayCallCount++ == 2)
                            {
                                context.IsPaused = true;
                            }
                        })
                    .Return(Task.FromResult(true));

                var sut = new WaitAction(delayService, TimeSpan.FromSeconds(50));

                var task = sut.ExecuteAsync(context);
                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));

                await context
                    .WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3))
                    .ToTask();

                Assert.True(context.IsPaused);
            }
        }
    }
}