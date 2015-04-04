namespace WorkoutWotch.UnitTests.Utility
{
    using System;
    using Xunit;

    public class MathExtFixture
    {
        [Fact]
        public void max_returns_first_if_second_is_null()
        {
            Assert.Equal("first", MathExt.Max("first", null));
        }

        [Fact]
        public void max_returns_second_if_first_is_null()
        {
            Assert.Equal("second", MathExt.Max(null, "second"));
        }

        [Fact]
        public void max_returns_the_maximum()
        {
            Assert.Equal(50, MathExt.Max(13, 50));
            Assert.Equal(50, MathExt.Max(50, 13));
            Assert.Equal(TimeSpan.FromSeconds(3), MathExt.Max(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(2100)));
            Assert.Equal(TimeSpan.FromSeconds(3), MathExt.Max(TimeSpan.FromMilliseconds(2100), TimeSpan.FromSeconds(3)));
        }

        [Fact]
        public void min_returns_first_if_it_is_null_and_second_is_not()
        {
            Assert.Null(MathExt.Min("first", null));
        }

        [Fact]
        public void min_returns_second_if_it_is_null_and_first_is_not()
        {
            Assert.Null(MathExt.Min(null, "second"));
        }

        [Fact]
        public void min_returns_the_minimum()
        {
            Assert.Equal(13, MathExt.Min(13, 50));
            Assert.Equal(13, MathExt.Min(50, 13));
            Assert.Equal(TimeSpan.FromMilliseconds(2100), MathExt.Min(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(2100)));
            Assert.Equal(TimeSpan.FromMilliseconds(2100), MathExt.Min(TimeSpan.FromMilliseconds(2100), TimeSpan.FromSeconds(3)));
        }
    }
}