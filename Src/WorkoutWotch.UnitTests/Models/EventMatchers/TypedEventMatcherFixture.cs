using System;
using NUnit.Framework;
using WorkoutWotch.Models.EventMatchers;
using WorkoutWotch.Models.Events;
using WorkoutWotch.Models;
using System.Linq;
using WorkoutWotch.UnitTests.Services.Logger.Mocks;
using Kent.Boogaart.PCLMock;

namespace WorkoutWotch.UnitTests.Models.EventMatchers
{
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
            Assert.False(sut.Matches(new AfterExerciseEvent(new ExecutionContext(), new Exercise(new LoggerServiceMock(MockBehavior.Loose), "name", 0, 0, Enumerable.Empty<MatcherWithAction>()))));
        }

        [Test]
        public void matches_returns_true_if_the_event_is_of_the_same_type()
        {
            var sut = new TypedEventMatcher<AfterExerciseEvent>();
            Assert.True(sut.Matches(new AfterExerciseEvent(new ExecutionContext(), new Exercise(new LoggerServiceMock(MockBehavior.Loose), "name", 0, 0, Enumerable.Empty<MatcherWithAction>()))));
        }

        [Test]
        public void matches_returns_true_if_the_event_is_of_a_derived_type()
        {
            var sut = new TypedEventMatcher<NumberedEvent>();
            Assert.True(sut.Matches(new BeforeSetEvent(new ExecutionContext(), 1)));
        }
    }
}

