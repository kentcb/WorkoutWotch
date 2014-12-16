namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class ExerciseProgramParserFixture
    {
        [Test]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramParser.GetParser(null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramParser.GetParser(new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [TestCase("# foo\n", "foo")]
        [TestCase("# Foo\n", "Foo")]
        [TestCase("# Foo bar\n", "Foo bar")]
        [TestCase("#    \t Foo   bar  \t \n", "Foo   bar")]
        public void can_parse_name(string input, string expectedName)
        {
            var result = ExerciseProgramParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedName, result.Name);
        }

        [TestCase("# ignore\n", 0)]
        [TestCase("# ignore\n## Exercise 1\n* 1 set x 1 rep\n", 1)]
        [TestCase("# ignore\n## Exercise 1\n* 1 set x 1 rep\n## Exercise 2\n* 1 set x 1 rep\n", 2)]
        [TestCase("# ignore\n## Exercise 1\n* 1 set x 1 rep\n\n\n\n## Exercise 2\n* 1 set x 1 rep\n", 2)]
        public void can_parse_exercises(string input, int expectedExerciseCount)
        {
            var result = ExerciseProgramParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedExerciseCount, result.Exercises.Count);
        }
    }
}