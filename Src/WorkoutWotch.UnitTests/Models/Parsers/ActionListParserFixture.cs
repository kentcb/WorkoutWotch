namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Logger.Mocks;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class ActionListParserFixture
    {
        [Fact]
        public void get_parser_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => ActionListParser.GetParser(-1, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [Theory]
        [InlineData(
            "* Wait for 2s\n* Break for 1m",
            0,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "* Wait for 2s  \n* Break for 1m   \t  ",
            0,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "* Say 'foo'\n* Wait for 1s\n* Wait for 2s\n* Break for 1m",
            0,
            new [] { typeof(SayAction), typeof(WaitAction), typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "  * Wait for 2s\n  * Break for 1m",
            1,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "\t* Wait for 2s\n\t* Break for 1m",
            1,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "\t* Wait for 2s\n  * Break for 1m",
            1,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "      * Wait for 2s\n      * Break for 1m",
            3,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "    *       Wait for 2s  \t \n    *  \t Break for 1m  \t\t  ",
            2,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [InlineData(
            "* Sequence:\n  * Say 'foo'\n  * Sequence:\n    * Say 'bar'",
            0,
            new [] { typeof(SequenceAction) })]
        public void can_parse_valid_input(string input, int indentLevel, Type[] expectedActionTypes)
        {
            var result = ActionListParser
                .GetParser(
                    indentLevel,
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))(new Input(input));

            Assert.True(result.WasSuccessful);
            Assert.True(result.Remainder.AtEnd);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Select(x => x.GetType()).SequenceEqual(expectedActionTypes));
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("* ", 0)]
        [InlineData("Wait for 2s", 0)]
        [InlineData("  * Wait for 2s", 0)]
        [InlineData("\t* Wait for 2s", 0)]
        [InlineData("* Wait for 2s", 1)]
        [InlineData("* Wait for 2s* Wait for 3s", 0)]
        public void cannot_parse_invalid_input(string input, int indentLevel)
        {
            var result = ActionListParser
                .GetParser(
                    indentLevel,
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}