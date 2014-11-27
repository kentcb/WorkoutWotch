namespace WorkoutWotch.Models
{
    public interface IEventMatcher
    {
        bool Matches(IEvent @event);
    }
}