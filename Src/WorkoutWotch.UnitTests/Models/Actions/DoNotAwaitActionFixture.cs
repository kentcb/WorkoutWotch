namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using Xunit;

    public sealed class DoNotAwaitActionFixture
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
    }
}