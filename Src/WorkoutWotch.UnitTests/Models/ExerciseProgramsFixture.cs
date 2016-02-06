namespace WorkoutWotch.UnitTests.Models
{
    using System.Linq;
    using Builders;
    using PCLMock;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Logger.Mocks;
    using Services.Speech.Mocks;
    using WorkoutWotch.Models;
    using Xunit;

    public class ExerciseProgramsFixture
    {
        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(10)]
        public void programs_yields_the_programs_passed_into_ctor(int programCount)
        {
            var programs = Enumerable.Range(0, programCount)
                .Select(x => new ExerciseProgramBuilder()
                    .WithName("Program " + x)
                    .Build())
                .ToList();
            var sut = new ExerciseProgramsBuilder()
                .WithPrograms(programs)
                .Build();

            Assert.Equal(programCount, sut.Programs.Count);
            Assert.True(sut.Programs.SequenceEqual(programs));
        }

        [Theory]
        [InlineData(
            "# first\n",
            new [] { "first" })]
        [InlineData(
            "# first\n# second\n# third\n",
            new [] { "first", "second", "third" })]
        public void parse_returns_an_appropriate_exercise_programs_instance(string input, string[] expectedProgramNames)
        {
            var result = ExercisePrograms.Parse(
                input,
                new AudioServiceMock(MockBehavior.Loose),
                new DelayServiceMock(MockBehavior.Loose),
                new LoggerServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose));

            Assert.NotNull(result);
            Assert.True(result.Programs.Select(x => x.Name).SequenceEqual(expectedProgramNames));
        }

        [Theory]
        [InlineData(
            "# first\n",
            new [] { "first" })]
        [InlineData(
            "# first\n# second\n# third\n",
            new [] { "first", "second", "third" })]
        public void try_parse_returns_an_appropriate_exercise_programs_instance(string input, string[] expectedProgramNames)
        {
            var result = ExercisePrograms.TryParse(
                input,
                new AudioServiceMock(MockBehavior.Loose),
                new DelayServiceMock(MockBehavior.Loose),
                new LoggerServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose));

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Programs.Select(x => x.Name).SequenceEqual(expectedProgramNames));
        }
    }
}