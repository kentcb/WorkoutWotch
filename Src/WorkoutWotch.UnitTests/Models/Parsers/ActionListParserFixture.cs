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
    public class ActionListParserFixture
    {
        [Test]
        public void get_parser_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => ActionListParser.GetParser(-1, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [TestCase(
            "* Wait for 2s\n* Break for 1m",
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "* Wait for 2s  \n* Break for 1m   \t  ",
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "* Say 'foo'\n* Wait for 1s\n* Wait for 2s\n* Break for 1m",
            new [] { typeof(SayAction), typeof(WaitAction), typeof(WaitAction), typeof(BreakAction) })]
        public void can_parse_valid_input(string input, Type[] expectedActionTypes)
        {
            var result = ActionListParser
                .GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.True(result.Select(x => x.GetType()).SequenceEqual(expectedActionTypes));
        }

        [TestCase("")]
        [TestCase("* ")]
        [TestCase("Wait for 2s")]
        [TestCase(" * Wait for 2s")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ActionListParser
                .GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())(new Input(input));

            Assert.False(result.WasSuccessful);
        }
    }
}