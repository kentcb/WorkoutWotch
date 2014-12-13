using System;
using System.Linq;
using NUnit.Framework;
using WorkoutWotch.UnitTests.Services.Audio.Mocks;
using WorkoutWotch.UnitTests.Services.Delay.Mocks;
using WorkoutWotch.UnitTests.Services.Logger.Mocks;
using WorkoutWotch.UnitTests.Services.Speech.Mocks;
using WorkoutWotch.Models.Parsers;
using Kent.Boogaart.PCLMock;
using Sprache;
using WorkoutWotch.Models.Actions;

namespace WorkoutWotch.UnitTests.Models.Parsers
{
    [TestFixture]
    public class SequenceActionParserFixture
    {
        [Test]
        public void get_parser_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => SequenceActionParser.GetParser(-1, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => SequenceActionParser.GetParser(0, null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => SequenceActionParser.GetParser(0, new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => SequenceActionParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => SequenceActionParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [TestCase(
            "Sequence:\n  * Say 'foo'\n  * Wait for 2s",
            0,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "Sequence:\n    * Say 'foo'\n    * Wait for 2s",
            1,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "Sequence:  \t  \n  * Say 'foo'\n  * Say 'bar'\n  * Say 'biz'",
            0,
            new [] { typeof(SayAction), typeof(SayAction), typeof(SayAction) })]
        public void can_parse_valid_input(string input, int indentLevel, Type[] expectedActionTypes)
        {
            var result = SequenceActionParser
                .GetParser(indentLevel, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.True(result.Children.Select(x => x.GetType()).SequenceEqual(expectedActionTypes));
        }

        [TestCase("Seequence:\n  * Say 'foo'", 0)]
        [TestCase("Sequence:\n", 0)]
        [TestCase("Sequence:\n  * Say 'foo'", 1)]
        public void cannot_parse_invalid_input(string input, int indentLevel)
        {
            var result = SequenceActionParser
                .GetParser(indentLevel, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}

