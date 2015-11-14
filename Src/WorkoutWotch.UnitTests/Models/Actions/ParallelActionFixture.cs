namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Builders;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using Xunit;

    public class ParallelActionFixture
    {
        [Fact]
        public void ctor_throws_if_children_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ParallelAction(null));
        }

        [Fact]
        public void ctor_throws_if_any_child_is_null()
        {
            Assert.Throws<ArgumentException>(() => new ParallelAction(new [] { new ActionMock(), null, new ActionMock() }));
        }

        [Fact]
        public void duration_is_zero_if_there_are_no_children()
        {
            var sut = new ParallelActionBuilder()
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void duration_is_the_maximum_duration_from_all_children()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            var action4 = new ActionMock();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromMilliseconds(150));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(3));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(5));

            action4
                .When(x => x.Duration)
                .Return(TimeSpan.FromMilliseconds(550));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .AddChild(action4)
                .Build();

            Assert.Equal(TimeSpan.FromSeconds(5), sut.Duration);
        }

        [Fact]
        public async Task execute_async_throws_if_the_execution_context_is_null()
        {
            var sut = new ParallelActionBuilder()
                .Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Fact]
        public async Task execute_async_executes_each_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var action4 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(1));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(2));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(3));

            action4
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(4));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .AddChild(action4)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                action1
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();

                action2
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();

                action3
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();

                action4
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public async Task execute_async_skips_actions_that_are_shorter_than_the_skip_ahead()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromMinutes(1));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(71));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(70)))
            {
                await sut.ExecuteAsync(context);

                action1
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action2
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action3
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public async Task execute_async_skips_actions_that_are_shorter_than_the_skip_ahead_even_if_the_execution_context_is_paused()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromMinutes(1));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(71));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(70)))
            {
                context.IsPaused = true;
                await sut.ExecuteAsync(context);

                action1
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action2
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action3
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public async Task execute_async_correctly_handles_a_skip_ahead_value_that_exceeds_even_the_longest_child_actions_duration()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromMinutes(1));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(71));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromMinutes(3)))
            {
                await sut.ExecuteAsync(context);

                action1
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action2
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action3
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                Assert.Equal(TimeSpan.FromSeconds(71), context.Progress);
            }
        }

        [Fact]
        public async Task execute_async_ensures_progress_of_child_actions_does_not_compound()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            var action4 = new ActionMock();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(1));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(1));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(2));

            action4
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(1));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action1.Duration))
                .Return(Observable.Return(Unit.Default));

            action2
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action2.Duration))
                .Return(Observable.Return(Unit.Default));

            action3
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action3.Duration))
                .Return(Observable.Return(Unit.Default));

            action4
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action4.Duration))
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .AddChild(action4)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                Assert.Equal(TimeSpan.FromSeconds(2), context.Progress);
            }
        }

        [Fact]
        public async Task execute_async_ensures_progress_of_child_actions_does_not_compound_when_skipping()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            var action4 = new ActionMock();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(3));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action4
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(4));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action1.Duration))
                .Return(Observable.Return(Unit.Default));

            action2
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action2.Duration))
                .Return(Observable.Return(Unit.Default));

            action3
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action3.Duration))
                .Return(Observable.Return(Unit.Default));

            action4
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action4.Duration))
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .AddChild(action3)
                .AddChild(action4)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(5)))
            {
                await sut.ExecuteAsync(context);

                Assert.Equal(TimeSpan.FromSeconds(10), context.Progress);

                action1
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action4
                    .Verify(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();
            }
        }

        [Fact]
        public async Task execute_async_context_can_be_cancelled_by_longest_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(13));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.Cancel())
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is cancelled
                Assert.True(context.IsCancelled);
            }
        }

        [Fact]
        public async Task execute_async_context_can_be_cancelled_by_child_action_that_is_not_the_longest()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(13));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.Cancel())
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is cancelled
                Assert.True(context.IsCancelled);
            }
        }

        [Fact]
        public async Task execute_async_context_can_be_paused_by_longest_child_action()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(13));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.IsPaused = true)
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is paused
                Assert.True(context.IsPaused);
            }
        }

        [Fact]
        public async Task execute_async_context_can_be_paused_by_child_action_that_is_not_the_longest()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(8));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(13));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.IsPaused = true)
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .AddChild(action1)
                .AddChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                await sut.ExecuteAsync(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is paused
                Assert.True(context.IsPaused);
            }
        }
    }
}