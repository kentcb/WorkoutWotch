namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using Xunit;

    public class MetronomeActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Fact]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => MetronomeActionParser.GetParser(null));
        }

        [Theory]
        [InlineData(
            "Metronome at 0s*, 1s, 1s, 2s-",
            new [] { 0, 1 * msInSecond, 1 * msInSecond, 2 * msInSecond },
            new [] { MetronomeTickType.Bell, MetronomeTickType.Click, MetronomeTickType.Click, MetronomeTickType.None })]
        [InlineData(
            "METRONOME At 0s*, 1s, 1s, 2s-",
            new [] { 0, 1 * msInSecond, 1 * msInSecond, 2 * msInSecond },
            new [] { MetronomeTickType.Bell, MetronomeTickType.Click, MetronomeTickType.Click, MetronomeTickType.None })]
        [InlineData(
            "Metronome  \t at   0s*   ,  \t  1s ,  \t 1s  ,  2s-",
            new [] { 0, 1 * msInSecond, 1 * msInSecond, 2 * msInSecond },
            new [] { MetronomeTickType.Bell, MetronomeTickType.Click, MetronomeTickType.Click, MetronomeTickType.None })]
        [InlineData(
            "Metronome at 0h 1m 2s, 2m 5s",
            new [] { 1 * msInMinute + 2 * msInSecond, 2 * msInMinute + 5 * msInSecond },
            new [] { MetronomeTickType.Click, MetronomeTickType.Click })]
        public void can_parse_correctly_formatted_input(string input, int[] expectedPeriodsBeforeMilliseconds, MetronomeTickType[] expectedTickTypes)
        {
            Assert.Equal(expectedPeriodsBeforeMilliseconds.Length, expectedTickTypes.Length);

            var result = MetronomeActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.NotNull(result);
            Assert.Equal(expectedPeriodsBeforeMilliseconds.Length, result.Ticks.Count);

            Assert.True(
                expectedPeriodsBeforeMilliseconds
                    .Select(x => TimeSpan.FromMilliseconds(x))
                    .SequenceEqual(
                        result
                            .Ticks
                            .Select(x => x.PeriodBefore)));
            Assert.True(
                expectedTickTypes
                    .SequenceEqual(
                        result
                            .Ticks
                            .Select(x => x.Type)));
        }

        [Theory]
        [InlineData("  Metronome at 1s")]
        [InlineData("Metronome at 1s\n")]
        [InlineData("Metronome at")]
        [InlineData("Metronome at abc")]
        [InlineData("Metronomeat 1s")]
        [InlineData("Metronome\n at 1s")]
        [InlineData("Metronome at\n 1s")]
        [InlineData("Metronome at 1s,\n2s")]
        public void cannot_parse_incorrectly_formatted_input(string input)
        {
            var result = MetronomeActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}