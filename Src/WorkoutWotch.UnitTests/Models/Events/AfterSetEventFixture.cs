namespace WorkoutWotch.UnitTests.Models.Events
{
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;

    [TestFixture]
    public class AfterSetEventFixture
    {
        [Test]
        public void to_string_returns_correct_representation(
            [Values(0, 1, 3, 8, 13, 1628)]int repetitions)
        {
            var sut = new AfterSetEvent(new ExecutionContext(), repetitions);

            Assert.AreEqual("After Set " + repetitions, sut.ToString());
        }
    }
}