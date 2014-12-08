using System;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.EventMatchers
{
    public sealed class TypedEventMatcher<T> : IEventMatcher
        where T : IEvent
    {
        public bool Matches(IEvent @event)
        {
            @event.AssertNotNull("@event");
            return @event is T;
        }
    }
}

