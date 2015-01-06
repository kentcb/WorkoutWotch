namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Models.Mocks;

    [TestFixture]
    public class SequenceActionFixture
    {
        [Test]
        public void ctor_throws_if_children_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SequenceAction(null));
        }

        [Test]
        public void duration_is_zero_if_there_are_no_child_actions()
        {
            var sut = new SequenceActionBuilder().Build();

            Assert.AreEqual(TimeSpan.Zero, sut.Duration);
        }

        [Test]
        public void duration_is_calculated_as_the_sum_of_child_durations()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(1));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(7));

            var sut = new SequenceActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            Assert.AreEqual(TimeSpan.FromSeconds(18), sut.Duration);
        }

        [Test]
        public void execute_async_throws_if_context_is_null()
        {
            var sut = new SequenceActionBuilder().Build();

            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_executes_each_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var sut = new SequenceActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                action1
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();

                action2
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();

                action3
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_child_actions_that_are_shorter_than_the_skip_ahead()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Throw();

            action2
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Throw();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(3));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(2));

            var sut = new SequenceActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(11)))
            {
                await sut.ExecuteAsync(context);

                action3
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_child_actions_that_are_shorter_than_the_skip_ahead_even_if_the_context_is_paused()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Throw();

            action2
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Throw();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(3));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(2));

            var sut = new SequenceActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(11)))
            {
                context.IsPaused = true;
                await sut.ExecuteAsync(context);

                action3
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();
            }
        }

        [Test]
        public void execute_async_stops_executing_if_the_context_is_cancelled()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var sut = new SequenceActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext())
            {
                action2
                    .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .Do(context.Cancel)
                    .Return(Task.FromResult(true));

                Assert.Throws<OperationCanceledException>(async () => await sut.ExecuteAsync(context));

                action1
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();

                action2
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasCalledExactlyOnce();

                action3
                    .Verify(x => x.ExecuteAsync(It.Is(context)))
                    .WasNotCalled();
            }
        }
    }
}