namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using Xunit;

    public class DoNotAwaitActionFixture
    {
        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new DoNotAwaitAction(null, new ActionMock()));
        }

        [Fact]
        public void ctor_throws_if_inner_action_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new DoNotAwaitAction(new LoggerServiceMock(), null));
        }

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
        public async Task execute_async_throws_if_the_context_is_null()
        {
            var sut = new DoNotAwaitActionBuilder().Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Fact]
        public void execute_async_does_not_wait_for_inner_actions_execution_to_complete_before_itself_completing()
        {
            var action = new ActionMock();
            var tcs = new TaskCompletionSource<bool>();

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return(tcs.Task);

            var sut = new DoNotAwaitActionBuilder()
                .WithInnerAction(action)
                .Build();

            var task = sut.ExecuteAsync(new ExecutionContext());

            Assert.True(task.Wait(TimeSpan.FromSeconds(3)));
        }

        [Fact]
        public async Task execute_async_logs_any_error_raised_by_the_inner_action()
        {
            var waitHandle = new ManualResetEventSlim();
            var logger = new LoggerMock(MockBehavior.Loose);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var action = new ActionMock(MockBehavior.Loose);

            logger
                .When(x => x.Error(It.IsAny<string>()))
                .Do(waitHandle.Set);

            loggerService
                .When(x => x.GetLogger(It.IsAny<Type>()))
                .Return(logger);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return(Task.Run(() => { throw new InvalidOperationException("Something bad happened"); }));

            var sut = new DoNotAwaitActionBuilder()
                .WithLoggerService(loggerService)
                .WithInnerAction(action)
                .Build();

            await sut.ExecuteAsync(new ExecutionContext());

            Assert.True(waitHandle.Wait(TimeSpan.FromSeconds(3)));
        }
    }
}