namespace WorkoutWotch.UnitTests.Models.Events
{
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

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
            var exercise = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(MockBehavior.Loose), exerciseName, 0, 0, Enumerable.Empty<MatcherWithAction>());
            var sut = new AfterExerciseEvent(new ExecutionContext(), exercise);

            Assert.AreEqual("After Exercise '" + exerciseName + "'", sut.ToString());
        }
    }
}