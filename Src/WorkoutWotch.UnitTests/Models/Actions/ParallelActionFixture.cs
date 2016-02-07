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

    public class ParallelActionFixture
    {
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
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .WithChild(action4)
                .Build();

            Assert.Equal(TimeSpan.FromSeconds(5), sut.Duration);
        }

        [Fact]
        public void execute_executes_each_child_action()
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
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .WithChild(action4)
                .Build();

            using (var context = new ExecutionContext())
            {
                sut.Execute(context);

                action1
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();

                action2
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();

                action3
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();

                action4
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public void execute_skips_actions_that_are_shorter_than_the_skip_ahead()
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
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(70)))
            {
                sut.Execute(context);

                action1
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action2
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action3
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public void execute_skips_actions_that_are_shorter_than_the_skip_ahead_even_if_the_execution_context_is_paused()
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
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(70)))
            {
                context.IsPaused = true;
                sut.Execute(context);

                action1
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action2
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action3
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public void execute_correctly_handles_a_skip_ahead_value_that_exceeds_even_the_longest_child_actions_duration()
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
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromMinutes(3)))
            {
                sut.Execute(context);

                action1
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action2
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action3
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                Assert.Equal(TimeSpan.FromSeconds(71), context.Progress);
            }
        }

        [Fact]
        public void execute_ensures_progress_of_child_actions_does_not_compound()
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
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action1.Duration))
                .Return(Observable.Return(Unit.Default));

            action2
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action2.Duration))
                .Return(Observable.Return(Unit.Default));

            action3
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action3.Duration))
                .Return(Observable.Return(Unit.Default));

            action4
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action4.Duration))
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .WithChild(action4)
                .Build();

            using (var context = new ExecutionContext())
            {
                sut.Execute(context);

                Assert.Equal(TimeSpan.FromSeconds(2), context.Progress);
            }
        }

        [Fact]
        public void execute_ensures_progress_of_child_actions_does_not_compound_when_skipping()
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
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action1.Duration))
                .Return(Observable.Return(Unit.Default));

            action2
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action2.Duration))
                .Return(Observable.Return(Unit.Default));

            action3
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action3.Duration))
                .Return(Observable.Return(Unit.Default));

            action4
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.AddProgress(action4.Duration))
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .WithChild(action1)
                .WithChild(action2)
                .WithChild(action3)
                .WithChild(action4)
                .Build();

            using (var context = new ExecutionContext(TimeSpan.FromSeconds(5)))
            {
                sut.Execute(context);

                Assert.Equal(TimeSpan.FromSeconds(10), context.Progress);

                action1
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();

                action4
                    .Verify(x => x.Execute(It.IsAny<ExecutionContext>()))
                    .WasNotCalled();
            }
        }

        [Fact]
        public void execute_context_can_be_cancelled_by_longest_child_action()
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
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.Cancel())
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .WithChild(action1)
                .WithChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                sut.Execute(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is cancelled
                Assert.True(context.IsCancelled);
            }
        }

        [Fact]
        public void execute_context_can_be_cancelled_by_child_action_that_is_not_the_longest()
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
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.Cancel())
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .WithChild(action1)
                .WithChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                sut.Execute(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is cancelled
                Assert.True(context.IsCancelled);
            }
        }

        [Fact]
        public void execute_context_can_be_paused_by_longest_child_action()
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
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.IsPaused = true)
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .WithChild(action1)
                .WithChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                sut.Execute(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is paused
                Assert.True(context.IsPaused);
            }
        }

        [Fact]
        public void execute_context_can_be_paused_by_child_action_that_is_not_the_longest()
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
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec => ec.IsPaused = true)
                .Return(Observable.Return(Unit.Default));

            var sut = new ParallelActionBuilder()
                .WithChild(action1)
                .WithChild(action2)
                .Build();

            using (var context = new ExecutionContext())
            {
                sut.Execute(context);

                // can't assume certain actions did not execute because actions run in parallel, but we can check the context is paused
                Assert.True(context.IsPaused);
            }
        }
    }
}