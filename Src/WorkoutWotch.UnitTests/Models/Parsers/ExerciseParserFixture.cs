namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using PCLMock;
    using Services.Audio.Mocks;
    using Services.Delay.Mocks;
    using Services.Logger.Mocks;
    using Services.Speech.Mocks;
    using Sprache;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Models.Parsers;
    using Xunit;

    public class ExerciseParserFixture
    {
        [Fact]
        public void get_parser_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(null, new DelayServiceMock(), new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(new AudioServiceMock(), null, new LoggerServiceMock(), new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), null, new SpeechServiceMock()));
        }

        [Fact]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [Theory]
        [InlineData("## foo\n* 1 set x 1 rep", "foo")]
        [InlineData("## Foo\n* 1 set x 1 rep", "Foo")]
        [InlineData("## Foo bar\n* 1 set x 1 rep", "Foo bar")]
        [InlineData("## !@$%^&*()-_=+[{]};:'\",<.>/?\n* 1 set x 1 rep", "!@$%^&*()-_=+[{]};:'\",<.>/?")]
        [InlineData("##    \t Foo   bar  \t \n* 1 set x 1 rep", "Foo   bar")]
        public void can_parse_name(string input, string expectedName)
        {
            var result = ExerciseParser
                .GetParser(
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);
        }

        [Theory]
        [InlineData("## ignore\n* 3 sets x 10 reps", 3, 10)]
        [InlineData("## ignore\n* 3 sets x 10 reps\n", 3, 10)]
        [InlineData("## ignore\n* 1 set x 1 rep", 1, 1)]
        [InlineData("## ignore\n* 1 set x 1 rep\n", 1, 1)]
        [InlineData("## ignore\n* 3 set x 1 reps", 3, 1)]
        [InlineData("## ignore\n* 3 setx1 reps", 3, 1)]
        [InlineData("## ignore\n* 3 Sets X 10 RepS", 3, 10)]
        [InlineData("## ignore\n*   \t  3  \t   sets \t x   10   \t\t reps  \t ", 3, 10)]
        [InlineData("## ignore\n\n\n \t\t   \t \n\n   \n* 3 sets x 10 reps ", 3, 10)]
        public void can_parse_set_and_repetition_counts(string input, int expectedSetCount, int expectedRepetitionCount)
        {
            var result = ExerciseParser
                .GetParser(
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.Equal(expectedSetCount, result.SetCount);
            Assert.Equal(expectedRepetitionCount, result.RepetitionCount);
        }

        [Theory]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n*  \t Before:\n  * Wait for 1s\n",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before  \t: \t  \n  * Wait for 1s\n",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n* Before:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>), typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n\n\n\n\n\n* Before:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>), typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n\n\n  \t\n \t\t  \n* Before:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>), typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<AfterExerciseEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before set:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n*  \t Before set:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before set \t:  \t \n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before \t  set \t:  \t \n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After set:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<AfterSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After \t  set:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<AfterSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before  \t rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* During rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<DuringRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* During \t  rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<DuringRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<AfterRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After \t  rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<AfterRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before set 1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before \t set \t 1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After set 1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<AfterSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After  \t set \t  1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<AfterSetEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before rep 1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before \t  rep \t  1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* During rep 1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<DuringRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* During \t  rep \t  1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<DuringRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After rep 1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<AfterRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* After \t  rep \t  1:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<AfterRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before set:\n  * Wait for 1s\n* Before rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeSetEvent>), typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* BEFORE SET:\n  * Wait for 1s\n* BeFOre Rep:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeSetEvent>), typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n* Before reps 1..3:\n  * Wait for 1s",
            new[] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [InlineData(
            "## ignore\n* 1 set x 1 rep\n\n\n \n  \t\t  \t \n\t   \n* Before:\n  * Wait for 1s",
            new[] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        public void can_parse_matchers_with_actions(string input, Type[] expectedMatcherTypes)
        {
            var result = ExerciseParser
                .GetParser(
                    new AudioServiceMock(MockBehavior.Loose),
                    new DelayServiceMock(MockBehavior.Loose),
                    new LoggerServiceMock(MockBehavior.Loose),
                    new SpeechServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.True(result.MatchersWithActions.Select(x => x.Matcher.GetType()).SequenceEqual(expectedMatcherTypes));
        }

        [Theory]
        [InlineData("# name\n* 1 set x 1 rep")]
        [InlineData("## name\n  * 1 set x 1 rep")]
        [InlineData("## name  * 1 set x 1 rep")]
        [InlineData("## name\n* abc set x 1 rep")]
        [InlineData("## name\n* 1 set x abc rep")]
        [InlineData("## name\n* 1.2 set x 1 rep")]
        [InlineData("## name\n* 1 set x 1.2 rep")]
        [InlineData("## name\n* 1 set")]
        [InlineData("## name\n* 1 rep")]
        [InlineData("##\n* 1 set x 1 rep")]
        [InlineData("## name\n1 set x 1 rep")]
        [InlineData("## name\n* 1\n set x 1 rep")]
        [InlineData("## name\n* 1 set\n x 1 rep")]
        [InlineData("## name\n* 1 set x\n 1 rep")]
        [InlineData("## name\n* 1 set x 1\n rep")]
        [InlineData("## name\n* 1 set x 1 rep\n*")]
        [InlineData("## name\n* 1 set x 1 rep\n* foo:")]
        [InlineData("## name\n* 1 set x 1 rep\n  * Before\n  * Wait for 1s")]
        [InlineData("## name\n* 1 set x 1 rep\n  * Before\n:  * Wait for 1s")]
        [InlineData("## name\n* 1 set x 1 rep\n  * Before:\n  * Wait for 1s")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ExerciseParser
               .GetParser(
                   new AudioServiceMock(MockBehavior.Loose),
                   new DelayServiceMock(MockBehavior.Loose),
                   new LoggerServiceMock(MockBehavior.Loose),
                   new SpeechServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}