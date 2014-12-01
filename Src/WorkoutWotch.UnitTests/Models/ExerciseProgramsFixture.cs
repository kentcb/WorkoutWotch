namespace WorkoutWotch.UnitTests.Models
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
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
            var exercisePrograms = new ExercisePrograms(programs);

            Assert.NotNull(exercisePrograms.Programs);
            Assert.AreSame(programs[0], exercisePrograms.Programs[0]);
            Assert.AreSame(programs[1], exercisePrograms.Programs[1]);
            Assert.AreSame(programs[2], exercisePrograms.Programs[2]);
        }

        #region Supporting Members

        private ExerciseProgram CreateExerciseProgram(string name)
        {
            return new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), name, Enumerable.Empty<Exercise>());
        }

        #endregion
    }
}