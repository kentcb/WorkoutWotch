namespace WorkoutWotch.Models.EventMatchers
{
    using Kent.Boogaart.HelperTrinity.Extensions;

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