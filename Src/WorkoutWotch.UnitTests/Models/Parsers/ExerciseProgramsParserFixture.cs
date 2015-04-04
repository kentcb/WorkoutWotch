namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Kent.Boogaart.PCLMock;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using Xunit;

    public class ExerciseProgramsParserFixture
    {
        [Fact]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(null));
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData(" \n\t\t  \n\n\n\n\t\n        \n", 0)]
        [InlineData("# first", 1)]
        [InlineData("# first\n", 1)]
        [InlineData("\n\n\n  \t\t \t \n\t\n\t\n\n   \n# first", 1)]
        [InlineData("# first\n# second", 2)]
        [InlineData("# first\n# second\n", 2)]
        [InlineData("# first\n\n\n# second\n", 2)]
        [InlineData("# first\n\n\n# second\n\n  \t \n  \t\t\t \n\n \t", 2)]
        public void can_parse_exercise_programs(string input, int expectedExerciseProgramCount)
        {
            var result = ExerciseProgramsParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.Equal(expectedExerciseProgramCount, result.Programs.Count);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("# first\n bla bla")]
        [InlineData("  # first")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ExerciseProgramsParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}