namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Linq;
    using Builders;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using Xunit;

    public class ExerciseProgramsFixture
    {
        [Fact]
        public void ctor_throws_if_programs_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExercisePrograms(null));
        }

        [Fact]
        public void ctor_throws_if_any_program_is_null()
        {
            Assert.Throws<ArgumentException>(() => new ExercisePrograms(new [] { new ExerciseProgramBuilder().Build(), null }));
        }

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
                .AddPrograms(programs)
                .Build();

            Assert.Equal(programCount, sut.Programs.Count);
            Assert.True(sut.Programs.SequenceEqual(programs));
        }

        [Fact]
        public void parse_throws_if_input_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.Parse(null, new ContainerServiceMock()));
        }

        [Fact]
        public void parse_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.Parse("input", null));
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
            var result = ExercisePrograms.Parse(input, new ContainerServiceMock(MockBehavior.Loose));

            Assert.NotNull(result);
            Assert.True(result.Programs.Select(x => x.Name).SequenceEqual(expectedProgramNames));
        }

        [Fact]
        public void try_parse_throws_if_input_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.TryParse(null, new ContainerServiceMock()));
        }

        [Fact]
        public void try_parse_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.TryParse("input", null));
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
            var result = ExercisePrograms.TryParse(input, new ContainerServiceMock(MockBehavior.Loose));

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Programs.Select(x => x.Name).SequenceEqual(expectedProgramNames));
        }
    }
}