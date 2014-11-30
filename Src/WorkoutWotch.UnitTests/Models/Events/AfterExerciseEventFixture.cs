namespace WorkoutWotch.UnitTests.Models.Events
{
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    [TestFixture]
    public class AfterExerciseEventFixture
    {
        [Test]
        public void to_string_returns_correct_representation(
            [Values("", "  ", "Push-ups", "Chin-ups", "Sit-ups")]string exerciseName)
        {
            var exercise = new Exercise(new LoggerServiceMock(MockBehavior.Loose), exerciseName, 0, 0, Enumerable.Empty<MatcherWithAction>());
            var @event = new AfterExerciseEvent(new ExecutionContext(), exercise);

            Assert.AreEqual("After Exercise '" + exerciseName + "'", @event.ToString());
        }
    }
}