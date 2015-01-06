namespace WorkoutWotch.UnitTests.Models.EventMatchers
{
    using System;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;

    [TestFixture]
    public class TypedEventMatcherFixture
    {
        [Test]
        public void matches_throws_if_event_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new TypedEventMatcher<BeforeExerciseEvent>().Matches(null));
        }

        [Test]
        public void matches_returns_false_if_the_event_is_of_a_different_type()
        {
            var sut = new TypedEventMatcher<BeforeExerciseEvent>();
            Assert.False(sut.Matches(new AfterExerciseEvent(new ExecutionContext(), new ExerciseBuilder().Build())));
        }

        [Test]
        public void matches_returns_true_if_the_event_is_of_the_same_type()
        {
            var sut = new TypedEventMatcher<AfterExerciseEvent>();
            Assert.True(sut.Matches(new AfterExerciseEvent(new ExecutionContext(), new ExerciseBuilder().Build())));
        }

        [Test]
        public void matches_returns_true_if_the_event_is_of_a_derived_type()
        {
            var sut = new TypedEventMatcher<NumberedEvent>();
            Assert.True(sut.Matches(new BeforeSetEvent(new ExecutionContext(), 1)));
        }
    }
}