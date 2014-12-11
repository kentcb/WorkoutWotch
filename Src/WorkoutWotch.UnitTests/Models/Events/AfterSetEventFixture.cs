namespace WorkoutWotch.UnitTests.Models.Events
{
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Events;

    [TestFixture]
    public class AfterSetEventFixture
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(8)]
        [TestCase(13)]
        [TestCase(1628)]
        public void to_string_returns_correct_representation(int repetitions)
        {
            var sut = new AfterSetEvent(new ExecutionContext(), repetitions);

            Assert.AreEqual("After Set " + repetitions, sut.ToString());
        }
    }
}