namespace WorkoutWotch.UnitTests.Models.Events
{
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;

    [TestFixture]
    public class AfterExerciseEventFixture
    {
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("Push-ups")]
        [TestCase("Chin-ups")]
        [TestCase("Sit-ups")]
        public void to_string_returns_correct_representation(string exerciseName)
        {
            var exercise = new ExerciseBuilder()
                .WithName(exerciseName)
                .Build();

            var sut = new AfterExerciseEvent(new ExecutionContext(), exercise);

            Assert.AreEqual("After Exercise '" + exerciseName + "'", sut.ToString());
        }
    }
}