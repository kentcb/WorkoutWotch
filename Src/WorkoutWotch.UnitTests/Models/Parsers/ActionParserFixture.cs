namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using PCLMock;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Logger.Mocks;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class ActionParserFixture
    {
        [Fact]
        public void ctor_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => ActionParser.GetParser(-1, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void ctor_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionParser.GetParser(0, null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void ctor_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionParser.GetParser(0, new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Fact]
        public void ctor_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionParser.GetParser(0, new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [Theory]
        [InlineData("Break for 10s", 0, typeof(BreakAction))]
        [InlineData("Metronome at 1s, 2s, 3s", 0, typeof(MetronomeAction))]
        [InlineData("Prepare for 10s", 0, typeof(PrepareAction))]
        [InlineData("Say 'foo'", 0, typeof(SayAction))]
        [InlineData("Wait for 10s", 0, typeof(WaitAction))]
        [InlineData("Sequence:\n  * Say 'foo'\n  * Say 'bar'", 0, typeof(SequenceAction))]
        [InlineData("Parallel:\n  * Say 'foo'\n  * Say 'bar'", 0, typeof(ParallelAction))]
        [InlineData("Sequence:\n      * Say 'foo'\n      * Say 'bar'", 2, typeof(SequenceAction))]
        [InlineData("Parallel:\n      * Say 'foo'\n      * Say 'bar'", 2, typeof(ParallelAction))]
        [InlineData("Don't wait:\n      * Say 'foo'\n      * Say 'bar'", 2, typeof(DoNotAwaitAction))]
        public void can_parse_valid_input(string input, int indentLevel, Type expectedType)
        {
            var result = ActionParser.GetParser(
                indentLevel,
                new AudioServiceMock(MockBehavior.Loose),
                new DelayServiceMock(MockBehavior.Loose),
                new LoggerServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.True(result.WasSuccessful);
            Assert.True(result.Remainder.AtEnd);
            Assert.NotNull(result.Value);
            Assert.Same(expectedType, result.Value.GetType());
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("* Say 'foo'")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ActionParser.GetParser(
                0,
                new AudioServiceMock(MockBehavior.Loose),
                new DelayServiceMock(MockBehavior.Loose),
                new LoggerServiceMock(MockBehavior.Loose),
                new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}