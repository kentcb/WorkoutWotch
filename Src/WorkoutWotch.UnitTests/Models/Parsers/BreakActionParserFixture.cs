using System;
using NUnit.Framework;
using WorkoutWotch.UnitTests.Services.Speech.Mocks;
using WorkoutWotch.Models.Parsers;
using WorkoutWotch.UnitTests.Services.Delay.Mocks;
using Sprache;

namespace WorkoutWotch.UnitTests.Models.Parsers
{
    [TestFixture]
    public class BreakActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BreakActionParser.GetParser(null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BreakActionParser.GetParser(new DelayServiceMock(), null));
        }

        [TestCase("Break for 24s", 24 * msInSecond)]
        [TestCase("Break for 1m 24s", 1 * msInMinute + 24 * msInSecond)]
        [TestCase("break For 24s", 24 * msInSecond)]
        [TestCase("Break   For \t 24s", 24 * msInSecond)]
        public void can_parse_valid_input(string input, int expectedMilliseconds)
        {
            var result = BreakActionParser.GetParser(new DelayServiceMock(), new SpeechServiceMock()).Parse(input);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [TestCase("Brake for 24s")]
        [TestCase("Breakfor 24s")]
        [TestCase("Break for")]
        [TestCase("Break for tea")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = BreakActionParser.GetParser(new DelayServiceMock(), new SpeechServiceMock())(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}

