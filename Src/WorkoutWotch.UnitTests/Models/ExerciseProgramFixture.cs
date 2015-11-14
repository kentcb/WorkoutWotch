namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Builders;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using Xunit;

    public class ExerciseProgramFixture
    {
        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgram(null, "name", Enumerable.Empty<Exercise>()));
        }

        [Fact]
        public void ctor_throws_if_name_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), null, Enumerable.Empty<Exercise>()));
        }

        [Fact]
        public void ctor_throws_if_exercises_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", null));
        }

        [Fact]
        public void ctor_throws_if_any_exercise_is_null()
        {
            Assert.Throws<ArgumentException>(() => new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", new Exercise[] { new ExerciseBuilder(), null }));
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Some longer name")]
        [InlineData("An exercise program name with !@*&(*$#&^$).,/.<?][:[]; weird characters")]
        public void name_yields_the_name_passed_into_ctor(string name)
        {
            var sut = new ExerciseProgramBuilder()
                .WithName(name)
                .Build();

            Assert.Equal(name, sut.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(10)]
        public void exercises_yields_the_exercises_passed_into_ctor(int exerciseCount)
        {
            var exercises = Enumerable.Range(0, exerciseCount)
                .Select(x => new ExerciseBuilder()
                    .WithName("Exercise " + x)
                    .Build())
                .ToList();
            var sut = new ExerciseProgramBuilder()
                .AddExercises(exercises)
                .Build();

            Assert.Equal(exerciseCount, sut.Exercises.Count);
            Assert.True(sut.Exercises.SequenceEqual(exercises));
        }

        [Fact]
        public void duration_is_zero_if_there_are_no_exercises()
        {
            var sut = new ExerciseProgramBuilder()
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void duration_is_the_sum_of_all_exercise_durations()
        {
            var action1 = new ActionMock();
            var action2 = new ActionMock();

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(1));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            var sut = new ExerciseProgramBuilder()
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action1))
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action2))
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action1))
                .Build();

            Assert.Equal(TimeSpan.FromSeconds(12), sut.Duration);
        }

        [Fact]
        public async Task execute_async_throws_if_the_context_is_null()
        {
            var sut = new ExerciseProgramBuilder()
                .Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Fact]
        public async Task execute_async_executes_each_exercise()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);

            var sut = new ExerciseProgramBuilder()
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action1))
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action2))
                .Build();

            using (var executionContext = new ExecutionContext())
            {
                await sut.ExecuteAsync(executionContext);

                action1
                    .Verify(x => x.ExecuteAsync(executionContext))
                    .WasCalledExactlyOnce();

                action2
                    .Verify(x => x.ExecuteAsync(executionContext))
                    .WasCalledExactlyOnce();
            }
        }

        [Fact]
        public async Task execute_async_skips_exercises_that_are_shorter_than_the_skip_ahead()
        {
            var action1 = new ActionMock(MockBehavior.Loose);
            var action2 = new ActionMock(MockBehavior.Loose);
            var action3 = new ActionMock(MockBehavior.Loose);

            action1
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(13));

            action2
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action3
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(5));

            action1
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Throw();

            action2
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Throw();

            var sut = new ExerciseProgramBuilder()
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action1))
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action2))
                .AddExercise(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action3))
                .Build();

            using (var executionContext = new ExecutionContext(TimeSpan.FromSeconds(23)))
            {
                await sut.ExecuteAsync(executionContext);

                action3
                    .Verify(x => x.ExecuteAsync(executionContext))
                    .WasCalledExactlyOnce();
            }
        }
    }
}