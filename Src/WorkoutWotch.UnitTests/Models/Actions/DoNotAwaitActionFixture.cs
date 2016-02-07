namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using Xunit;

    public class DoNotAwaitActionFixture
    {
        [Fact]
        public void duration_always_returns_zero_regardless_of_inner_action_duration()
        {
            var action = new ActionMock();

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(3));

            var sut = new DoNotAwaitActionBuilder()
                .WithInnerAction(action)
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void execute_does_not_wait_for_inner_actions_execution_to_complete_before_itself_completing()
        {
            var action = new ActionMock();

            action
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Return(Observable.Never<Unit>());

            var sut = new DoNotAwaitActionBuilder()
                .WithInnerAction(action)
                .Build();

            var completed = false;
            sut
                .Execute(new ExecutionContext())
                .Subscribe(_ => completed = true);

            Assert.True(completed);
        }

#if DEBUG

        [Fact]
        public void execute_logs_any_error_raised_by_the_inner_action()
        {
            var waitHandle = new ManualResetEventSlim();
            var logger = new LoggerMock(MockBehavior.Loose);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var action = new ActionMock(MockBehavior.Loose);

            logger
                .When(x => x.Log(LogLevel.Error, It.IsAny<string>()))
                .Do(waitHandle.Set);

            loggerService
                .When(x => x.GetLogger(It.IsAny<Type>()))
                .Return(logger);

            action
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Return(Observable.Throw<Unit>(new InvalidOperationException("Something bad happened")));

            var sut = new DoNotAwaitActionBuilder()
                .WithLoggerService(loggerService)
                .WithInnerAction(action)
                .Build();

            sut.Execute(new ExecutionContext());

            Assert.True(waitHandle.Wait(TimeSpan.FromSeconds(3)));
        }

#endif
    }
}