namespace WorkoutWotch.UnitTests.Models.Events
{
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;
    using Xunit;

    public class BeforeExerciseEventFixture
    {
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("Push-ups")]
        [InlineData("Chin-ups")]
        [InlineData("Sit-ups")]
        public void to_string_returns_correct_representation(string exerciseName)
        {
            var exercise = new ExerciseBuilder()
                .WithName(exerciseName)
                .Build();

            var sut = new BeforeExerciseEvent(new ExecutionContext(), exercise);

            Assert.Equal("Before Exercise '" + exerciseName + "'", sut.ToString());
        }
    }
}