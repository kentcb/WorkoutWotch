using System;
using NUnit.Framework;
using WorkoutWotch.Models;
using System.Linq;
using WorkoutWotch.UnitTests.Models.Mocks;
using ReactiveUI;
using System.Collections.Generic;
using Kent.Boogaart.PCLMock;
using WorkoutWotch.Models.Events;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace WorkoutWotch.UnitTests.Models
{
    [TestFixture]
    public class ExerciseFixture
    {
        [Test]
        public void ctor_throws_if_name_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new Exercise(null, 3, 10, Enumerable.Empty<MatcherWithAction>()));
        }

        [Test]
        public void ctor_throws_if_set_count_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => new Exercise("name", -3, 10, Enumerable.Empty<MatcherWithAction>()));
        }

        [Test]
        public void ctor_throws_if_repetition_count_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => new Exercise("name", 3, -10, Enumerable.Empty<MatcherWithAction>()));
        }

        [Test]
        public void ctor_throws_if_matchers_with_actions_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new Exercise("name", 3, 10, null));
        }

        [Test]
        public void name_gets_name_passed_into_ctor()
        {
            var exercise = new Exercise("some name", 3, 10, Enumerable.Empty<MatcherWithAction>());
            Assert.AreEqual("some name", exercise.Name);
        }

        [Test]
        public void set_count_gets_set_count_passed_into_ctor()
        {
            var exercise = new Exercise("some name", 3, 10, Enumerable.Empty<MatcherWithAction>());
            Assert.AreEqual(3, exercise.SetCount);
        }

        [Test]
        public void repetition_count_gets_repetition_count_passed_into_ctor()
        {
            var exercise = new Exercise("some name", 3, 10, Enumerable.Empty<MatcherWithAction>());
            Assert.AreEqual(10, exercise.RepetitionCount);
        }

        [Test]
        public void duration_returns_zero_if_there_are_no_actions()
        {
            var exercise = new Exercise("some name", 3, 10, Enumerable.Empty<MatcherWithAction>());
            Assert.AreEqual(TimeSpan.Zero, exercise.Duration);
        }

        [Test]
        public void duration_returns_sum_of_action_durations()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            var action3 = new ActionMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(3));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            var eventMatcher1 = new EventMatcherMock();
            var eventMatcher2 = new EventMatcherMock();
            var eventMatcher3 = new EventMatcherMock();
            eventMatcher1.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            eventMatcher2.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is DuringRepetitionEvent);
            eventMatcher3.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is AfterSetEvent);
            var matchersWithActions = new List<MatcherWithAction>
            {
                new MatcherWithAction(eventMatcher1, action1),
                new MatcherWithAction(eventMatcher2, action2),
                new MatcherWithAction(eventMatcher3, action3),
            };
            var exercise = new Exercise("name", 2, 3, matchersWithActions);
            Assert.AreEqual(TimeSpan.FromSeconds(30), exercise.Duration);
        }

        [Test]
        public void execute_async_throws_if_execution_context_is_null()
        {
            var exercise = new Exercise("name", 2, 3, Enumerable.Empty<MatcherWithAction>());
            Assert.Throws<ArgumentNullException>(async () => await exercise.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_executes_all_appropriate_actions()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var eventMatcher1 = new EventMatcherMock();
            var eventMatcher2 = new EventMatcherMock();
            var eventMatcher3 = new EventMatcherMock();
            eventMatcher1.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            eventMatcher2.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is DuringRepetitionEvent);
            eventMatcher3.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is AfterSetEvent);
            var matchersWithActions = new List<MatcherWithAction>
            {
                new MatcherWithAction(eventMatcher1, action1),
                new MatcherWithAction(eventMatcher2, action2),
                new MatcherWithAction(eventMatcher3, action3),
            };
            var exercise = new Exercise("name", 2, 3, matchersWithActions);

            using (var executionContext = new ExecutionContext())
            {
                await exercise.ExecuteAsync(executionContext);

                action1.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
                action2.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactly(times: 6);
                action3.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactly(times: 2);
            }
        }

        [Test]
        public async Task execute_async_does_not_skip_zero_duration_actions()
        {
            var action = new ActionMock(MockBehavior.Loose);
            var eventMatcher = new EventMatcherMock();
            eventMatcher.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            var matchersWithActions = new List<MatcherWithAction>
            {
                new MatcherWithAction(eventMatcher, action)
            };
            var exercise = new Exercise("name", 2, 3, matchersWithActions);

            using (var executionContext = new ExecutionContext())
            {
                await exercise.ExecuteAsync(executionContext);
                action.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_actions_that_are_shorter_than_the_skip_ahead()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var eventMatcher1 = new EventMatcherMock();
            var eventMatcher2 = new EventMatcherMock();
            var eventMatcher3 = new EventMatcherMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(3));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Throw();
            action2.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Throw();
            eventMatcher1.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            eventMatcher2.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            eventMatcher3.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            var matchersWithActions = new List<MatcherWithAction>
            {
                new MatcherWithAction(eventMatcher1, action1),
                new MatcherWithAction(eventMatcher2, action2),
                new MatcherWithAction(eventMatcher3, action3),
            };
            var exercise = new Exercise("name", 2, 3, matchersWithActions);

            using (var executionContext = new ExecutionContext(TimeSpan.FromSeconds(13)))
            {
                await exercise.ExecuteAsync(executionContext);

                action3.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_actions_that_are_shorter_than_the_skip_ahead_even_if_the_context_is_paused()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            var eventMatcher1 = new EventMatcherMock();
            var eventMatcher2 = new EventMatcherMock();
            var eventMatcher3 = new EventMatcherMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(3));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Throw();
            action2.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Throw();
            eventMatcher1.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            eventMatcher2.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            eventMatcher3.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
            var matchersWithActions = new List<MatcherWithAction>
            {
                new MatcherWithAction(eventMatcher1, action1),
                new MatcherWithAction(eventMatcher2, action2),
                new MatcherWithAction(eventMatcher3, action3),
            };
            var exercise = new Exercise("name", 2, 3, matchersWithActions);

            using (var executionContext = new ExecutionContext(TimeSpan.FromSeconds(13)))
            {
                executionContext.IsPaused = true;
                await exercise.ExecuteAsync(executionContext);

                action3.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_updates_the_current_exercise_in_the_context()
        {
            var exercise = new Exercise("name", 2, 3, Enumerable.Empty<MatcherWithAction>());
            var context = new ExecutionContext();

            await exercise.ExecuteAsync(context);

            Assert.AreSame(exercise, context.CurrentExercise);
        }

        [Test]
        public async Task execute_async_updates_the_current_set_in_the_context()
        {
            var exercise = new Exercise("name", 3, 5, Enumerable.Empty<MatcherWithAction>());
            var context = new ExecutionContext();
            var currentSetsTask = context
                .ObservableForProperty(x => x.CurrentSet)
                .Select(x => x.Value)
                .Take(3)
                .ToListAsync()
                .ToTask();

            await exercise.ExecuteAsync(context);
            var currentSets = await currentSetsTask;

            Assert.AreEqual(3, currentSets.Count);
            Assert.AreEqual(1, currentSets[0]);
            Assert.AreEqual(2, currentSets[1]);
            Assert.AreEqual(3, currentSets[2]);
        }

        [Test]
        public async Task execute_async_updates_the_current_repetitions_in_the_context()
        {
            var exercise = new Exercise("name", 3, 5, Enumerable.Empty<MatcherWithAction>());
            var context = new ExecutionContext();
            var currentRepetitionsTask = context
                .ObservableForProperty(x => x.CurrentRepetition)
                .Select(x => x.Value)
                .Take(5)
                .ToListAsync()
                .ToTask();

            await exercise.ExecuteAsync(context);
            var currentRepetitions = await currentRepetitionsTask;

            Assert.AreEqual(5, currentRepetitions.Count);
            Assert.AreEqual(1, currentRepetitions[0]);
            Assert.AreEqual(2, currentRepetitions[1]);
            Assert.AreEqual(3, currentRepetitions[2]);
            Assert.AreEqual(4, currentRepetitions[3]);
            Assert.AreEqual(5, currentRepetitions[4]);
        }
    }
}

