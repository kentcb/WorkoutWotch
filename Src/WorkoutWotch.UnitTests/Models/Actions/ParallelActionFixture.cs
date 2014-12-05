namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Models.Mocks;

    [TestFixture]
    public class ParallelActionFixture
    {
        [Test]
        public void ctor_throws_if_children_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ParallelAction(null));
        }

        [Test]
        public void duration_is_zero_if_there_are_no_children()
        {
            Assert.AreEqual(TimeSpan.Zero, new ParallelAction(Enumerable.Empty<IAction>()).Duration);
        }

        [Test]
        public void duration_is_the_maximum_duration_from_all_children()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            var action4 = new ActionMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromMilliseconds(150));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(3));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(5));
            action4.When(x => x.Duration).Return(TimeSpan.FromMilliseconds(550));

            var sut = new ParallelAction(new [] { action1, action2, action3, action4 });
            Assert.AreEqual(TimeSpan.FromSeconds(5), sut.Duration);
        }

        [Test]
        public void execute_async_throws_if_the_execution_context_is_null()
        {
            var sut = new ParallelAction(Enumerable.Empty<IAction>());
            Assert.Throws<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_executes_each_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var action4 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(2));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(3));
            action4.When(x => x.Duration).Return(TimeSpan.FromSeconds(4));
            var sut = new ParallelAction(new [] { action1, action2, action3, action4 });

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                action1.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasCalledExactlyOnce();
                action2.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasCalledExactlyOnce();
                action3.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasCalledExactlyOnce();
                action4.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_actions_that_are_shorter_than_the_skip_ahead()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromMinutes(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(71));
            var sut = new ParallelAction(new [] { action1, action2, action3 });

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(70)))
            {
                await sut.ExecuteAsync(context);

                action1.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action2.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action3.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_actions_that_are_shorter_than_the_skip_ahead_even_if_the_execution_context_is_paused()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromMinutes(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(71));
            var sut = new ParallelAction(new [] { action1, action2, action3 });

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(70)))
            {
                context.IsPaused = true;
                await sut.ExecuteAsync(context);

                action1.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action2.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action3.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_correctly_handles_a_skip_ahead_value_that_exceeds_even_the_longest_child_actions_duration()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromMinutes(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(71));
            var sut = new ParallelAction(new [] { action1, action2, action3 });

            using (var context = new ExecutionContext(TimeSpan.FromMinutes(3)))
            {
                await sut.ExecuteAsync(context);

                action1.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action2.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action3.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();

                Assert.AreEqual(TimeSpan.FromSeconds(71), context.Progress);
            }
        }

        [Test]
        public async Task execute_async_ensures_progress_of_child_actions_does_not_compound()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            var action4 = new ActionMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(2));
            action4.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action1.Duration)).Return(Task.FromResult(true));
            action2.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action2.Duration)).Return(Task.FromResult(true));
            action3.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action3.Duration)).Return(Task.FromResult(true));
            action4.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action4.Duration)).Return(Task.FromResult(true));
            var sut = new ParallelAction(new [] { action1, action2, action3, action4 });

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                Assert.AreEqual(TimeSpan.FromSeconds(2), context.Progress);
            }
        }

        [Test]
        public async Task execute_async_ensures_progress_of_child_actions_does_not_compound_when_skipping()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            var action4 = new ActionMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(3));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(8));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action4.When(x => x.Duration).Return(TimeSpan.FromSeconds(4));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action1.Duration)).Return(Task.FromResult(true));
            action2.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action2.Duration)).Return(Task.FromResult(true));
            action3.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action3.Duration)).Return(Task.FromResult(true));
            action4.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.AddProgress(action4.Duration)).Return(Task.FromResult(true));
            var sut = new ParallelAction(new [] { action1, action2, action3, action4 });

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(5)))
            {
                await sut.ExecuteAsync(context);

                Assert.AreEqual(TimeSpan.FromSeconds(10), context.Progress);

                action1.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
                action4.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).WasNotCalled();
            }
        }

        [Test]
        public async Task execute_async_context_can_be_cancelled_by_longest_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(13));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(8));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.Cancel()).Return(Task.FromResult(true));
            var sut = new ParallelAction(new [] { action1, action2 });

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is cancelled
                Assert.True(context.IsCancelled);
            }
        }

        [Test]
        public async Task execute_async_context_can_be_cancelled_by_child_action_that_is_not_the_longest()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(8));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(13));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.Cancel()).Return(Task.FromResult(true));
            var sut = new ParallelAction(new [] { action1, action2 });

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is cancelled
                Assert.True(context.IsCancelled);
            }
        }

        [Test]
        public async Task execute_async_context_can_be_paused_by_longest_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(13));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(8));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.IsPaused = true).Return(Task.FromResult(true));
            var sut = new ParallelAction(new [] { action1, action2 });

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is paused
                Assert.True(context.IsPaused);
            }
        }

        [Test]
        public async Task execute_async_context_can_be_paused_by_child_action_that_is_not_the_longest()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(8));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(13));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Do<ExecutionContext>(ec => ec.IsPaused = true).Return(Task.FromResult(true));
            var sut = new ParallelAction(new [] { action1, action2 });

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is paused
                Assert.True(context.IsPaused);
            }
        }
    }
}