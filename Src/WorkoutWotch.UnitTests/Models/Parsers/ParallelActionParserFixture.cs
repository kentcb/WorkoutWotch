namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class ParallelActionParserFixture
    {
        [Test]
        public void get_parser_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => ParallelActionParser.GetParser(-1, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ParallelActionParser.GetParser(0, null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ParallelActionParser.GetParser(0, new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ParallelActionParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ParallelActionParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [TestCase(
            "Parallel:\n  * Say 'foo'\n  * Wait for 2s",
            0,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "Parallel:\n    * Say 'foo'\n    * Wait for 2s",
            1,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "Parallel:  \t  \n  * Say 'foo'\n  * Say 'bar'\n  * Say 'biz'",
            0,
            new [] { typeof(SayAction), typeof(SayAction), typeof(SayAction) })]
        public void can_parse_valid_input(string input, int indentLevel, Type[] expectedActionTypes)
        {
            var result = ParallelActionParser
                .GetParser(indentLevel, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.True(result.Children.Select(x => x.GetType()).SequenceEqual(expectedActionTypes));
        }

        [TestCase("Parallal:\n  * Say 'foo'", 0)]
        [TestCase("Parallel:\n", 0)]
        [TestCase("Parallel:\n  * Say 'foo'", 1)]
        public void cannot_parse_invalid_input(string input, int indentLevel)
        {
            var result = ParallelActionParser
                .GetParser(indentLevel, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}