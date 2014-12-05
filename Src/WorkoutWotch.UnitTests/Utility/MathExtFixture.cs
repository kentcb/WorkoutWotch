namespace WorkoutWotch.UnitTests.Utility
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class MathExtFixture
    {
        [Test]
        public void max_returns_first_if_second_is_null()
        {
            Assert.AreEqual("first", MathExt.Max("first", null));
        }

        [Test]
        public void max_returns_second_if_first_is_null()
        {
            Assert.AreEqual("second", MathExt.Max(null, "second"));
        }

        [Test]
        public void max_returns_the_maximum()
        {
            Assert.AreEqual(50, MathExt.Max(13, 50));
            Assert.AreEqual(50, MathExt.Max(50, 13));
            Assert.AreEqual(TimeSpan.FromSeconds(3), MathExt.Max(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(2100)));
            Assert.AreEqual(TimeSpan.FromSeconds(3), MathExt.Max(TimeSpan.FromMilliseconds(2100), TimeSpan.FromSeconds(3)));
        }

        [Test]
        public void min_returns_first_if_it_is_null_and_second_is_not()
        {
            Assert.IsNull(MathExt.Min("first", null));
        }

        [Test]
        public void min_returns_second_if_it_is_null_and_first_is_not()
        {
            Assert.IsNull(MathExt.Min(null, "second"));
        }

        [Test]
        public void min_returns_the_minimum()
        {
            Assert.AreEqual(13, MathExt.Min(13, 50));
            Assert.AreEqual(13, MathExt.Min(50, 13));
            Assert.AreEqual(TimeSpan.FromMilliseconds(2100), MathExt.Min(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(2100)));
            Assert.AreEqual(TimeSpan.FromMilliseconds(2100), MathExt.Min(TimeSpan.FromMilliseconds(2100), TimeSpan.FromSeconds(3)));
        }
    }
}