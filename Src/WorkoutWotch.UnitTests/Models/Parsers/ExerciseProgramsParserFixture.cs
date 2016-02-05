namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using PCLMock;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Logger.Mocks;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class ExerciseProgramsParserFixture
    {
        [Fact]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseProgramsParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
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
                .GetParser(
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))
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
                .GetParser(
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}