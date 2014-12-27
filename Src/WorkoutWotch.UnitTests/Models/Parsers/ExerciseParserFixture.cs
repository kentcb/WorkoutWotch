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
    using WorkoutWotch.UnitTests.Services.Container.Mocks;

    [TestFixture]
    public class ExerciseParserFixture
    {
        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ExerciseParser.GetParser(null));
        }

        [TestCase("## foo\n* 1 set x 1 rep", "foo")]
        [TestCase("## Foo\n* 1 set x 1 rep", "Foo")]
        [TestCase("## Foo bar\n* 1 set x 1 rep", "Foo bar")]
        [TestCase("## !@$%^&*()-_=+[{]};:'\",<.>/?\n* 1 set x 1 rep", "!@$%^&*()-_=+[{]};:'\",<.>/?")]
        [TestCase("##    \t Foo   bar  \t \n* 1 set x 1 rep", "Foo   bar")]
        public void can_parse_name(string input, string expectedName)
        {
            var result = ExerciseParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedName, result.Name);
        }

        [TestCase("## ignore\n* 3 sets x 10 reps", 3, 10)]
        [TestCase("## ignore\n* 3 sets x 10 reps\n", 3, 10)]
        [TestCase("## ignore\n* 1 set x 1 rep", 1, 1)]
        [TestCase("## ignore\n* 1 set x 1 rep\n", 1, 1)]
        [TestCase("## ignore\n* 3 set x 1 reps", 3, 1)]
        [TestCase("## ignore\n* 3 setx1 reps", 3, 1)]
        [TestCase("## ignore\n* 3 Sets X 10 RepS", 3, 10)]
        [TestCase("## ignore\n*   \t  3  \t   sets \t x   10   \t\t reps  \t ", 3, 10)]
        [TestCase("## ignore\n\n\n \t\t   \t \n\n   \n* 3 sets x 10 reps ", 3, 10)]
        public void can_parse_set_and_repetition_counts(string input, int expectedSetCount, int expectedRepetitionCount)
        {
            var result = ExerciseParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.AreEqual(expectedSetCount, result.SetCount);
            Assert.AreEqual(expectedRepetitionCount, result.RepetitionCount);
        }

        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n*  \t Before:\n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before  \t: \t  \n  * Wait for 1s\n",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n* Before:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>), typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n\n\n\n\n\n* Before:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>), typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before:\n  * Wait for 1s\n\n\n  \t\n \t\t  \n* Before:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>), typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<AfterExerciseEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n*  \t Before set:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set \t:  \t \n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before \t  set \t:  \t \n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After set:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<AfterSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After \t  set:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<AfterSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before  \t rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* During rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<DuringRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* During \t  rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<DuringRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<AfterRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After \t  rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<AfterRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before \t set \t 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After set 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<AfterSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After  \t set \t  1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<AfterSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before rep 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before \t  rep \t  1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* During rep 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<DuringRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* During \t  rep \t  1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<DuringRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After rep 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<AfterRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* After \t  rep \t  1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<AfterRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set:\n  * Wait for 1s\n* Before rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>), typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* BEFORE SET:\n  * Wait for 1s\n* BeFOre Rep:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeSetEvent>), typeof(TypedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before set 1:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<BeforeSetEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n* Before reps 1..3:\n  * Wait for 1s",
            new [] { typeof(NumberedEventMatcher<BeforeRepetitionEvent>) })]
        [TestCase(
            "## ignore\n* 1 set x 1 rep\n\n\n \n  \t\t  \t \n\t   \n* Before:\n  * Wait for 1s",
            new [] { typeof(TypedEventMatcher<BeforeExerciseEvent>) })]
        public void can_parse_matchers_with_actions(string input, Type[] expectedMatcherTypes)
        {
            var result = ExerciseParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            Assert.True(result.MatchersWithActions.Select(x => x.Matcher.GetType()).SequenceEqual(expectedMatcherTypes));
        }

        [TestCase("# name\n* 1 set x 1 rep")]
        [TestCase("## name\n  * 1 set x 1 rep")]
        [TestCase("## name  * 1 set x 1 rep")]
        [TestCase("## name\n* abc set x 1 rep")]
        [TestCase("## name\n* 1 set x abc rep")]
        [TestCase("## name\n* 1.2 set x 1 rep")]
        [TestCase("## name\n* 1 set x 1.2 rep")]
        [TestCase("## name\n* 1 set")]
        [TestCase("## name\n* 1 rep")]
        [TestCase("##\n* 1 set x 1 rep")]
        [TestCase("## name\n1 set x 1 rep")]
        [TestCase("## name\n* 1\n set x 1 rep")]
        [TestCase("## name\n* 1 set\n x 1 rep")]
        [TestCase("## name\n* 1 set x\n 1 rep")]
        [TestCase("## name\n* 1 set x 1\n rep")]
        [TestCase("## name\n* 1 set x 1 rep\n*")]
        [TestCase("## name\n* 1 set x 1 rep\n* foo:")]
        [TestCase("## name\n* 1 set x 1 rep\n  * Before\n  * Wait for 1s")]
        [TestCase("## name\n* 1 set x 1 rep\n  * Before\n:  * Wait for 1s")]
        [TestCase("## name\n* 1 set x 1 rep\n  * Before:\n  * Wait for 1s")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = ExerciseParser
                .GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}