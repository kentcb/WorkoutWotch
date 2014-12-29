namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;

    [TestFixture]
    public class ExerciseProgramsFixture
    {
        [Test]
        public void ctor_throws_if_programs_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExercisePrograms(null));
        }

        [Test]
        public void ctor_throws_if_any_program_is_null()
        {
            Assert.Throws<ArgumentException>(() => new ExercisePrograms(
                new []
                {
                    this.CreateExerciseProgram("first"),
                    null
                }));
        }

        [Test]
        public void programs_contains_programs_passed_into_ctor()
        {
            var programs = new[]
            {
                this.CreateExerciseProgram("first"),
                this.CreateExerciseProgram("second"),
                this.CreateExerciseProgram("third")
            };
            var sut = new ExercisePrograms(programs);

            Assert.NotNull(sut.Programs);
            Assert.AreSame(programs[0], sut.Programs[0]);
            Assert.AreSame(programs[1], sut.Programs[1]);
            Assert.AreSame(programs[2], sut.Programs[2]);
        }

        [Test]
        public void parse_throws_if_input_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.Parse(null, new ContainerServiceMock()));
        }

        [Test]
        public void parse_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.Parse("input", null));
        }

        [TestCase(
            "# first\n",
            new [] { "first" })]
        [TestCase(
            "# first\n# second\n# third\n",
            new [] { "first", "second", "third" })]
        public void parse_returns_an_appropriate_exercise_programs_instance(string input, string[] expectedProgramNames)
        {
            var result = ExercisePrograms.Parse(input, new ContainerServiceMock(MockBehavior.Loose));

            Assert.NotNull(result);
            Assert.True(result.Programs.Select(x => x.Name).SequenceEqual(expectedProgramNames));
        }

        [Test]
        public void try_parse_throws_if_input_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.TryParse(null, new ContainerServiceMock()));
        }

        [Test]
        public void try_parse_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExercisePrograms.TryParse("input", null));
        }

        [TestCase(
            "# first\n",
            new [] { "first" })]
        [TestCase(
            "# first\n# second\n# third\n",
            new [] { "first", "second", "third" })]
        public void try_parse_returns_an_appropriate_exercise_programs_instance(string input, string[] expectedProgramNames)
        {
            var result = ExercisePrograms.TryParse(input, new ContainerServiceMock(MockBehavior.Loose));

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Programs.Select(x => x.Name).SequenceEqual(expectedProgramNames));
        }

        #region Supporting Members

        private ExerciseProgram CreateExerciseProgram(string name)
        {
            return new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), name, Enumerable.Empty<Exercise>());
        }

        #endregion
    }
}