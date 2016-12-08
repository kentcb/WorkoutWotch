namespace WorkoutWotch.Models.EventMatchers
{
    using Genesis.Ensure;

    public sealed class TypedEventMatcher<T> : IEventMatcher
        where T : IEvent
    {
        public bool Matches(IEvent @event)
        {
            Ensure.ArgumentNotNull(@event, nameof(@event));
            return @event is T;
        }
    }
}