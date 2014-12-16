namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class ExerciseParserFixture
    {
        [Test]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [TestCase("## foo\n* 1 set x 1 rep\n", "foo")]
        [TestCase("## Foo\n* 1 set x 1 rep\n", "Foo")]
        [TestCase("## Foo bar\n* 1 set x 1 rep\n", "Foo bar")]
        [TestCase("##    \t Foo   bar  \t \n* 1 set x 1 rep\n", "Foo   bar")]
        public void can_parse_name(string input, string expectedName)
        {
            var result = ExerciseParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedName, result.Name);
        }

        [TestCase("## ignore\n* 3 sets x 10 reps\n", 3, 10)]
        [TestCase("## ignore\n* 1 set x 1 rep\n", 1, 1)]
        [TestCase("## ignore\n* 3 set x 1 reps\n", 3, 1)]
        [TestCase("## ignore\n* 3 Sets X 10 Reps\n", 3, 10)]
        public void can_parse_set_and_repetition_counts(string input, int expectedSetCount, int expectedRepetitionCount)
        {
            var result = ExerciseParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedSetCount, result.SetCount);
            Assert.AreEqual(expectedRepetitionCount, result.RepetitionCount);
        }

        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<AfterExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After set:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<AfterSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before rep:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* During rep:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<DuringRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After rep:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<AfterRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set 1:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After set 1:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<AfterSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before rep 1:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* During rep 1:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<DuringRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After rep 1:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<AfterRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set:\n  * Wait for 1s\n* Before rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>), typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* BEFORE SET:\n  * Wait for 1s\n* BeFOre Rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>), typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set 1:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before reps 1..3:\n  * Wait for 1s\n",
            new [] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        public void can_parse_matchers_with_actions(string input, Type[] expectedMatcherTypes)
        {
            var result = ExerciseParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())
                .Parse(input);

            Assert.NotNull(result);
            Assert.True(result.MatchersWithActions.Select(x => x.Matcher.GetType()).SequenceEqual(expectedMatcherTypes));
        }

        [TestCase("# only one hash\n* 1 set x 1 rep")]
        [TestCase("##\n* 1 set x 1 rep")]
        [TestCase("## name\n* 1 set")]
        [TestCase("## name\n* 1 rep")]
        [TestCase("## name\n* 1 set x 1 rep\n* foo:")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ExerciseParser
                .GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock())(new Input(input));
            Assert.True(!result.WasSuccessful || !result.Remainder.AtEnd);
        }
    }
}