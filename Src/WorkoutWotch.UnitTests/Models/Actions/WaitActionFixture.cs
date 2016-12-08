namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Builders;
    using global::ReactiveUI;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using Xunit;

    public sealed class WaitActionFixture
    {
        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(23498)]
        public void duration_yields_the_duration_passed_into_ctor(int delayInMs)
        {
            var sut = new WaitActionBuilder()
                .WithDelay(TimeSpan.FromMilliseconds(delayInMs))
                .Build();

            Assert.Equal(TimeSpan.FromMilliseconds(delayInMs), sut.Duration);
        }

        [Fact]
        public void execute_completes_even_if_there_is_no_delay()
        {
            var sut = new WaitActionBuilder()
                .WithDelay(TimeSpan.Zero)
                .Build();

            var executionContext = new ExecutionContext();
            var completed = false;
            sut
                .Execute(executionContext)
                .Subscribe(_ => completed = true);

            Assert.True(completed);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(23498)]
        public void execute_breaks_for_the_specified_delay(int delayInMs)
        {
            var delayService = new DelayServiceMock();
            var totalDelay = TimeSpan.Zero;

            delayService
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Do<TimeSpan>(t => totalDelay += t)
                .Return(Observables.Unit);

            var sut = new WaitActionBuilder()
                .WithDelayService(delayService)
                .WithDelay(TimeSpan.FromMilliseconds(delayInMs))
                .Build();

            sut
                .Execute(new ExecutionContext())
                .Subscribe();

            Assert.Equal(TimeSpan.FromMilliseconds(delayInMs), totalDelay);
        }

        [Theory]
        [InlineData(850, 800, 50)]
        [InlineData(850, 849, 1)]
        [InlineData(3478, 2921, 557)]
        public void execute_skips_ahead_if_the_context_has_skip_ahead(int delayInMs, int skipInMs, int expectedDelayInMs)
        {
            var delayService = new DelayServiceMock();
            var totalDelay = TimeSpan.Zero;

            delayService
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Do<TimeSpan>(t => totalDelay += t)
                .Return(Observables.Unit);

            var sut = new WaitActionBuilder()
                .WithDelayService(delayService)
                .WithDelay(TimeSpan.FromMilliseconds(delayInMs))
                .Build();

            sut
                .Execute(new ExecutionContext(TimeSpan.FromMilliseconds(skipInMs)))
                .Subscribe();

            Assert.Equal(TimeSpan.FromMilliseconds(expectedDelayInMs), totalDelay);
        }

        [Theory]
        [InlineData(850, 800)]
        [InlineData(850, 849)]
        [InlineData(3478, 2921)]
        public void execute_skips_ahead_if_the_context_has_skip_ahead_even_if_the_context_is_paused(int delayInMs, int skipInMs)
        {
            var delayService = new DelayServiceMock();
            var totalDelay = TimeSpan.Zero;

            delayService
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Do<TimeSpan>(t => totalDelay += t)
                .Return(Observables.Unit);

            var sut = new WaitActionBuilder()
                .WithDelayService(delayService)
                .WithDelay(TimeSpan.FromMilliseconds(delayInMs))
                .Build();

            var context = new ExecutionContext(TimeSpan.FromMilliseconds(skipInMs))
            {
                IsPaused = true
            };
            var progress = context
                .WhenAnyValue(x => x.Progress)
                .Skip(1)
                .CreateCollection();

            sut
                .Execute(context)
                .Subscribe();

            Assert.Equal(TimeSpan.FromMilliseconds(skipInMs), progress.First());
        }

        [Fact]
        public void execute_reports_progress()
        {
            var delayService = new DelayServiceMock(MockBehavior.Loose);
            var sut = new WaitActionBuilder()
                .WithDelayService(delayService)
                .WithDelay(TimeSpan.FromMilliseconds(50))
                .Build();
            var context = new ExecutionContext();

            Assert.Equal(TimeSpan.Zero, context.Progress);

            sut
                .Execute(context)
                .Subscribe();

            Assert.Equal(TimeSpan.FromMilliseconds(50), context.Progress);
        }

        [Fact]
        public void execute_reports_progress_correctly_even_if_the_skip_ahead_exceeds_the_wait_duration()
        {
            var delayService = new DelayServiceMock(MockBehavior.Loose);
            var sut = new WaitActionBuilder()
                .WithDelayService(delayService)
                .WithDelay(TimeSpan.FromMilliseconds(50))
                .Build();
            var context = new ExecutionContext(TimeSpan.FromMilliseconds(100));

            Assert.Equal(TimeSpan.Zero, context.Progress);

            sut
                .Execute(context)
                .Subscribe();

            Assert.Equal(TimeSpan.FromMilliseconds(50), context.Progress);
        }

        [Fact]
        public void execute_pauses_if_context_is_paused()
        {
            var delayService = new DelayServiceMock();
            var delayCallCount = 0;
            var context = new ExecutionContext();

            delayService
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Do(() => context.IsPaused = delayCallCount++ == 2)
                .Return(Observables.Unit);

            var sut = new WaitActionBuilder()
                .WithDelayService(delayService)
                .WithDelay(TimeSpan.FromSeconds(50))
                .Build();

            sut
                .Execute(context)
                .Subscribe();

            Assert.True(context.IsPaused);
            delayService
                .Verify(x => x.Delay(It.IsAny<TimeSpan>()))
                .WasCalledExactly(times: 3);
        }
    }
}