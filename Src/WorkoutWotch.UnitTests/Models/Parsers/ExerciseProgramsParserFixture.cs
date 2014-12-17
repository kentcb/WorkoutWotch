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
    public class ExerciseProgramsParserFixture
    {
        [Test]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [TestCase("", 0)]
        [TestCase("# first\n", 1)]
        [TestCase("# first\n# second\n", 2)]
        [TestCase("# first\n\n\n# second\n", 2)]
        [TestCase("# first\n\n\n# second\n\n  \t \n  \t\t\t \n\n \t", 2)]
        public void can_parse_exercise_programs(string input, int expectedExerciseProgramCount)
        {
            var result = ExerciseProgramsParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedExerciseProgramCount, result.Programs.Count);
        }

        [TestCase("abc")]
        [TestCase("# first\n bla bla")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ExerciseProgramsParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())(new Input(input));
            Assert.True(!result.WasSuccessful || !result.Remainder.AtEnd);
        }
    }
}