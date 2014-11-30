using System;
using WorkoutWotch.Models;
using Kent.Boogaart.PCLMock;

namespace WorkoutWotch.UnitTests.Models.Mocks
{
    public sealed class EventMatcherMock : MockBase<IEventMatcher>, IEventMatcher
    {
        public EventMatcherMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
        }

        public bool Matches(IEvent @event)
        {
            return this.Apply(x => x.Matches(@event));
        }
    }
}

