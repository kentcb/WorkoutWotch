namespace WorkoutWotch.Models.EventMatchers
{
    using System;
    using Utility;
    using WorkoutWotch.Models.Events;

    public sealed class NumberedEventMatcher<T> : IEventMatcher
        where T : NumberedEvent
    {
        private readonly Func<T, bool> matches;

        public NumberedEventMatcher(Func<T, bool> matches)
        {
            Ensure.ArgumentNotNull(matches, nameof(matches));
            this.matches = matches;
        }

        public bool Matches(IEvent @event)
        {
            var castEvent = @event as T;

            if (castEvent == null)
            {
                return false;
            }

            return this.matches(castEvent);
        }
    }
}