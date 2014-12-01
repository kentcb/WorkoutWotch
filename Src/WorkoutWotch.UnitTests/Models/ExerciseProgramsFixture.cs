using System;
using NUnit.Framework;
using WorkoutWotch.Models;
using Kent.Boogaart.PCLMock;
using System.Linq;
using WorkoutWotch.UnitTests.Services.Logger.Mocks;

namespace WorkoutWotch.UnitTests.Models
{
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
                    new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "first", Enumerable.Empty<Exercise>()),
                    null
                }));
        }

        [Test]
        public void programs_contains_programs_passed_into_ctor()
        {
            var programs = new[]
            {
                new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "first", Enumerable.Empty<Exercise>()),
                new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "second", Enumerable.Empty<Exercise>()),
                new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "third", Enumerable.Empty<Exercise>())
            };
            var exercisePrograms = new ExercisePrograms(programs);

            Assert.NotNull(exercisePrograms.Programs);
            Assert.AreSame(programs[0], exercisePrograms.Programs[0]);
            Assert.AreSame(programs[1], exercisePrograms.Programs[1]);
            Assert.AreSame(programs[2], exercisePrograms.Programs[2]);
        }
    }
}

