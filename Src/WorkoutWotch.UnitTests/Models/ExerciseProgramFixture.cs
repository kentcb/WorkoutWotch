namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    [TestFixture]
    public class ExerciseProgramFixture
    {
        [Test]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgram(null, "name", Enumerable.Empty<Exercise>()));
        }

        [Test]
        public void ctor_throws_if_name_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), null, Enumerable.Empty<Exercise>()));
        }

        [Test]
        public void ctor_throws_if_exercises_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", null));
        }

        [Test]
        public void ctor_throws_if_any_exercise_is_null()
        {
            Assert.Throws<ArgumentException>(() => new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", new Exercise[] { this.CreateExercise("name"), null }));
        }

        [Test]
        public void name_returns_value_provided_to_ctor()
        {
            Assert.AreEqual("some name", new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "some name", Enumerable.Empty<Exercise>()).Name);
        }

        [Test]
        public void exercises_returns_exercises_provided_to_ctor()
        {
            var exercises = new List<Exercise>
            {
                this.CreateExercise("first"),
                this.CreateExercise("second"),
                this.CreateExercise("third")
            };
            var exerciseProgram = new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", exercises);

            Assert.AreEqual(3, exerciseProgram.Exercises.Count);
            Assert.AreSame(exercises[0], exerciseProgram.Exercises[0]);
            Assert.AreSame(exercises[1], exerciseProgram.Exercises[1]);
            Assert.AreSame(exercises[2], exerciseProgram.Exercises[2]);
        }

        [Test]
        public void duration_is_zero_if_there_are_no_exercises()
        {
            Assert.AreEqual(TimeSpan.Zero, new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", Enumerable.Empty<Exercise>()).Duration);
        }

        [Test]
        public void duration_is_the_sum_of_all_exercise_durations()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            var exercises = new []
            {
                this.CreateExercise("first", action1),
                this.CreateExercise("second", action2),
                this.CreateExercise("third", action1)
            };
            var exerciseProgram = new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", exercises);

            Assert.AreEqual(TimeSpan.FromSeconds(12), exerciseProgram.Duration);
        }

        [Test]
        public void execute_async_throws_if_the_context_is_null()
        {
            var exerciseProgram = new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", Enumerable.Empty<Exercise>());
            Assert.Throws<ArgumentNullException>(async () => await exerciseProgram.ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_executes_each_exercise()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(1));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            var exercises = new []
            {
                this.CreateExercise("first", action1),
                this.CreateExercise("second", action2)
            };
            var exerciseProgram = new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", exercises);

            using (var executionContext = new ExecutionContext())
            {
                await exerciseProgram.ExecuteAsync(executionContext);

                action1.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
                action2.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
            }
        }

        [Test]
        public async Task execute_async_skips_exercises_that_are_shorter_than_the_skip_ahead()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);
            action1.When(x => x.Duration).Return(TimeSpan.FromSeconds(13));
            action2.When(x => x.Duration).Return(TimeSpan.FromSeconds(10));
            action3.When(x => x.Duration).Return(TimeSpan.FromSeconds(5));
            action1.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Throw();
            action2.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Throw();
            var exercises = new []
            {
                this.CreateExercise("first", action1),
                this.CreateExercise("second", action2),
                this.CreateExercise("third", action3)
            };
            var exerciseProgram = new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", exercises);

            using (var executionContext = new ExecutionContext(TimeSpan.FromSeconds(23)))
            {
                await exerciseProgram.ExecuteAsync(executionContext);

                action3.Verify(x => x.ExecuteAsync(It.Is(executionContext))).WasCalledExactlyOnce();
            }
        }

        #region Supporting Members

        private Exercise CreateExercise(string name, IAction beforeExerciseAction = null)
        {
            var matchers = new List<MatcherWithAction>();

            if (beforeExerciseAction != null)
            {
                var matcher = new EventMatcherMock();
                matcher.When(x => x.Matches(It.IsAny<IEvent>())).Return((IEvent @event) => @event is BeforeExerciseEvent);
                matchers.Add(new MatcherWithAction(matcher, beforeExerciseAction));
            }

            return new Exercise(new LoggerServiceMock(MockBehavior.Loose), name, 0, 0, matchers);
        }

        #endregion
    }
}