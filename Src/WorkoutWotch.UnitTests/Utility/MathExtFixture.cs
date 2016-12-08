namespace WorkoutWotch.UnitTests.Utility
{
    using System;
    using Xunit;

    public sealed class MathExtFixture
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(0, 1, 1)]
        [InlineData(100, 200, 200)]
        [InlineData(200, 100, 200)]
        [InlineData(100, -200, 100)]
        public void max_returns_the_maximum_value_for_value_types(int first, int second, int expected)
        {
            Assert.Equal(expected, MathExt.Max(first, second));
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("foo", null, "foo")]
        [InlineData(null, "foo", "foo")]
        [InlineData("foo", "foo", "foo")]
        [InlineData("foo", "bar", "foo")]
        [InlineData("bar", "foo", "foo")]
        public void max_returns_the_maximum_value_for_reference_types(string first, string second, string expected)
        {
            Assert.Equal(expected, MathExt.Max(first, second));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(100, 200, 100)]
        [InlineData(200, 100, 100)]
        [InlineData(100, -200, -200)]
        public void min_returns_the_minimum_value_for_value_types(int first, int second, int expected)
        {
            Assert.Equal(expected, MathExt.Min(first, second));
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("foo", null, null)]
        [InlineData(null, "foo", null)]
        [InlineData("foo", "foo", "foo")]
        [InlineData("foo", "bar", "bar")]
        [InlineData("bar", "foo", "bar")]
        public void min_returns_the_minimum_value_for_reference_types(string first, string second, string expected)
        {
            Assert.Equal(expected, MathExt.Min(first, second));
        }
    }
}