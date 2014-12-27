namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;

    [TestFixture]
    public class ExerciseProgramsParserFixture
    {
        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(null));
        }

        [TestCase("", 0)]
        [TestCase(" \n\t\t  \n\n\n\n\t\n        \n", 0)]
        [TestCase("# first", 1)]
        [TestCase("# first\n", 1)]
        [TestCase("\n\n\n  \t\t \t \n\t\n\t\n\n   \n# first", 1)]
        [TestCase("# first\n# second", 2)]
        [TestCase("# first\n# second\n", 2)]
        [TestCase("# first\n\n\n# second\n", 2)]
        [TestCase("# first\n\n\n# second\n\n  \t \n  \t\t\t \n\n \t", 2)]
        public void can_parse_exercise_programs(string input, int expectedExerciseProgramCount)
        {
            var result = ExerciseProgramsParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedExerciseProgramCount, result.Programs.Count);
        }

        [TestCase("abc")]
        [TestCase("# first\n bla bla")]
        [TestCase("  # first")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ExerciseProgramsParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}