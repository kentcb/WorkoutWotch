namespace WorkoutWotch.UnitTests.Models.EventMatchers
{
    using System;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using Xunit;

    public class NumberedEventMatcherFixture
    {
        [Fact]
        public void matches_returns_false_if_event_type_differs()
        {
            var sut = new NumberedEventMatcher<BeforeSetEvent>(_ => true);
            Assert.False(sut.Matches(new AfterSetEvent(new ExecutionContext(), 1)));
        }

        [Fact]
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