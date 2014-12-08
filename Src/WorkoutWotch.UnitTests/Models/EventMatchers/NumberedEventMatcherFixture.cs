namespace WorkoutWotch.UnitTests.Models.EventMatchers
{
    using System;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;

    [TestFixture]
    public class NumberedEventMatcherFixture
    {
        [Test]
        public void ctor_throws_if_matches_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new NumberedEventMatcher<BeforeSetEvent>(null));
        }

        [Test]
        public void matches_returns_false_if_event_type_differs()
        {
            var sut = new NumberedEventMatcher<BeforeSetEvent>(_ => true);
            Assert.False(sut.Matches(new AfterSetEvent(new ExecutionContext(), 1)));
        }

        [Test]
        public void matches_invokes_matches_function_to_obtain_result()
        {
            var called = false;
            Func<BeforeSetEvent, bool> matches = x =>
            {
                called = true;
                return true;
            };
            var sut = new NumberedEventMatcher<BeforeSetEvent>(matches);
            Assert.True(sut.Matches(new BeforeSetEvent(new ExecutionContext(), 1)));
            Assert.True(called);
        }
    }
}